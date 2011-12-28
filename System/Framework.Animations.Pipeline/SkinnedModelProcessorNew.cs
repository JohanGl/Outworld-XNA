using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Framework.Animations.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Framework.Animations.Pipeline
{
	/// <summary>
	/// Custom processor extends the builtin framework ModelProcessor class,
	/// adding animation support.
	/// </summary>
	[ContentProcessor(DisplayName = "SkinnedModelNew - Framework.Animations.Pipeline")]
	public class SkinnedModelProcessorNew : ModelProcessor
	{
		//[DefaultValue(1.0f)]
		//[DisplayName("Animation Speed")]
		//[Description("Sets the speed of the animation")]
		//public float AnimationSpeed { get; set; }

		/// <summary>
		/// The main Process method converts an intermediate format content pipeline
		/// NodeContent tree to a ModelContent object with embedded animation data.
		/// </summary>
		public override ModelContent Process(NodeContent input, ContentProcessorContext context)
		{
			ValidateMesh(input, context, null);

			//ReadOpaqueData(input);

			// Find the skeleton.
			BoneContent skeleton = MeshHelper.FindSkeleton(input);

			if (skeleton == null)
			{
				throw new InvalidContentException("Input skeleton not found.");
			}

			// We don't want to have to worry about different parts of the model being
			// in different local coordinate systems, so let's just bake everything.
			FlattenTransforms(input, skeleton);

			// Read the bind pose and skeleton hierarchy data.
			IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

			if (bones.Count > SkinnedEffect.MaxBones)
			{
				throw new InvalidContentException(string.Format("Skeleton has {0} bones, but the maximum supported is {1}.", bones.Count, SkinnedEffect.MaxBones));
			}

			var bindPose = new List<Matrix>();
			var inverseBindPose = new List<Matrix>();
			var skeletonHierarchy = new List<int>();

			foreach (BoneContent bone in bones)
			{
				bindPose.Add(bone.Transform);
				inverseBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
				skeletonHierarchy.Add(bones.IndexOf(bone.Parent as BoneContent));
			}

			// Convert animation data to our runtime format.
			var animationClips = ProcessAnimations(skeleton.Animations, bones, context, input.Identity);

			// Chain to the base ModelProcessor class so it can convert the model data.
			ModelContent model = base.Process(input, context);

			// Store our custom animation data in the Tag property of the model.
			model.Tag = new SkinningData(animationClips, bindPose, inverseBindPose, skeletonHierarchy);

			return model;
		}

		// TODO: Use for sub-animations
		private void ReadOpaqueData(NodeContent node)
		{
			if (node.OpaqueData.Count > 0)
			{
				int a = 10;
				//var a = node.OpaqueData.GetValue<string>("AnimName", null);
			}
		}

		/// <summary>
		/// Converts an intermediate format content pipeline AnimationContentDictionary
		/// object to our runtime AnimationClip format.
		/// </summary>
		static Dictionary<string, AnimationClip> ProcessAnimations(AnimationContentDictionary animations, IList<BoneContent> bones, ContentProcessorContext context, ContentIdentity sourceIdentity)
		{
			// Build up a table mapping bone names to indices.
			var boneMap = new Dictionary<string, int>();

			for (int i = 0; i < bones.Count; i++)
			{
				string boneName = bones[i].Name;

				if (!string.IsNullOrEmpty(boneName))
				{
					boneMap.Add(boneName, i);
				}
			}

			// Convert each animation in turn.
			var animationClips = new Dictionary<string, AnimationClip>();

			foreach (KeyValuePair<string, AnimationContent> animation in animations)
			{
				AnimationClip processed = ProcessAnimation(animation.Value, boneMap);
				animationClips.Add(animation.Key, processed);
			}

			// Check to see if there's an animation clip definition
			// Here, we're checking for a file with the _Anims suffix.
			// So, if your model is named dude.fbx, we'd add dude_Anims.xml in the same folder
			// and the pipeline will see the file and use it to override the animations in the
			// original model file.
			string sourceModelFile = sourceIdentity.SourceFilename;
			string sourcePath = Path.GetDirectoryName(sourceModelFile);
			string animFilename = Path.GetFileNameWithoutExtension(sourceModelFile) + "_Anims.xml";
			string animPath = Path.Combine(sourcePath, animFilename);
			if (File.Exists(animPath))
			{
				// Add the filename as a dependency, so if it changes, the model is rebuilt
				context.AddDependency(animPath);

				// Load the animation definition from the XML file
				AnimationDefinition animationDefinition = context.BuildAndLoadAsset<XmlImporter, AnimationDefinition>(new ExternalReference<XmlImporter>(animPath), null);

				// Break up the original animation clips into our new clips
				// First, we check if the clips contains our clip to break up
				if (animationClips.ContainsKey(animationDefinition.OriginalClipName))
				{
					// Grab the main clip that we are using
					AnimationClip mainClip = animationClips[animationDefinition.OriginalClipName];

					// Now remove the original clip from our animations
					animationClips.Remove(animationDefinition.OriginalClipName);

					// Process each of our new animation clip parts
					foreach (AnimationDefinition.ClipPart clipPart in animationDefinition.ClipParts)
					{
						long animationDuration = mainClip.Duration.Ticks;

						// Calculate the frame times
						TimeSpan startTime = GetTimeSpanForFrame(clipPart.StartFrame, animationDefinition.OriginalFrameCount, animationDuration);
						TimeSpan endTime = GetTimeSpanForFrame(clipPart.EndFrame, animationDefinition.OriginalFrameCount, animationDuration);

						// Get all the keyframes for the animation clip that fall within the start and end time
						var keyframes = new List<Keyframe>();
						foreach (Keyframe keyframe in mainClip.Keyframes)
						{
							if ((keyframe.Time >= startTime) && (keyframe.Time <= endTime))
							{
								var frameDuration = TimeSpan.FromTicks((long)((keyframe.Time - startTime).Ticks * clipPart.Duration));

								//long animationDuration = (long)(MainClip.Duration.Ticks * Part.Speed);
								var newFrame = new Keyframe(keyframe.Bone, frameDuration, keyframe.Transform);
								keyframes.Add(newFrame);
							}
						}

						// Process the events
						var events = new List<AnimationEvent>();
						if (clipPart.Events != null)
						{
							// Process each event
							foreach (AnimationDefinition.ClipPart.Event clipEvent in clipPart.Events)
							{
								// Get the event time within the animation
								TimeSpan eventTime = GetTimeSpanForFrame(clipEvent.Keyframe, animationDefinition.OriginalFrameCount, animationDuration);

								// Offset the event time so it is relative to the start of the animation
								eventTime -= startTime;

								// Create the event
								var newEvent = new AnimationEvent();
								newEvent.EventTime = eventTime;
								newEvent.EventName = clipEvent.Name;
								events.Add(newEvent);
							}
						}

						var clipDuration = TimeSpan.FromTicks((long)((endTime - startTime).Ticks * clipPart.Duration));

						// Create the clip
						var newClip = new AnimationClip(clipDuration, keyframes, events, clipPart.ClipName);
						animationClips[clipPart.ClipName] = newClip;
					}
				}
			}

			if (animationClips.Count == 0)
			{
				throw new InvalidContentException("Input file does not contain any animations.");
			}

			return animationClips;
		}

		/// <summary>
		/// Converts an intermediate format content pipeline AnimationContent
		/// object to our runtime AnimationClip format.
		/// </summary>
		static AnimationClip ProcessAnimation(AnimationContent animation, Dictionary<string, int> boneMap)
		{
			var keyframes = new List<Keyframe>();

			// For each input animation channel.
			foreach (KeyValuePair<string, AnimationChannel> channel in animation.Channels)
			{
				// Look up what bone this channel is controlling.
				int boneIndex;

				if (!boneMap.TryGetValue(channel.Key, out boneIndex))
				{
					throw new InvalidContentException(string.Format("Found animation for bone '{0}', which is not part of the skeleton.", channel.Key));
				}

				// Convert the keyframe data.
				foreach (AnimationKeyframe keyframe in channel.Value)
				{
					keyframes.Add(new Keyframe(boneIndex, keyframe.Time, keyframe.Transform));
				}
			}

			// Sort the merged keyframes by time.
			keyframes.Sort(CompareKeyframeTimes);

			if (keyframes.Count == 0)
			{
				throw new InvalidContentException("Animation has no keyframes.");
			}

			if (animation.Duration <= TimeSpan.Zero)
			{
				throw new InvalidContentException("Animation has a zero duration.");
			}

			return new AnimationClip(animation.Duration, keyframes);
		}

		/// <summary>
		/// Gets a TimeSpan value for a frame index in an animation
		/// </summary>
		private static TimeSpan GetTimeSpanForFrame(int frameIndex, int totalFrameCount, long totalTicks)
		{
			float maxFrameIndex = (float)totalFrameCount - 1;
			float amountOfAnimation = (float)frameIndex / maxFrameIndex;
			float numTicks = amountOfAnimation * (float)totalTicks;
			return new TimeSpan((long)numTicks);
		}

		/// <summary>
		/// Comparison function for sorting keyframes into ascending time order.
		/// </summary>
		static int CompareKeyframeTimes(Keyframe a, Keyframe b)
		{
			return a.Time.CompareTo(b.Time);
		}

		/// <summary>
		/// Makes sure this mesh contains the kind of data we know how to animate.
		/// </summary>
		static void ValidateMesh(NodeContent node, ContentProcessorContext context, string parentBoneName)
		{
			var mesh = node as MeshContent;

			if (mesh != null)
			{
				// Validate the mesh.
				if (parentBoneName != null)
				{
					context.Logger.LogWarning(null, null, "Mesh {0} is a child of bone {1}. SkinnedModelProcessor does not correctly handle meshes that are children of bones.", mesh.Name, parentBoneName);
				}

				if (!MeshHasSkinning(mesh))
				{
					context.Logger.LogWarning(null, null, "Mesh {0} has no skinning information, so it has been deleted.", mesh.Name);
					mesh.Parent.Children.Remove(mesh);
					return;
				}
			}
			else if (node is BoneContent)
			{
				// If this is a bone, remember that we are now looking inside it.
				parentBoneName = node.Name;
			}

			// Recurse (iterating over a copy of the child collection,
			// because validating children may delete some of them).
			foreach (NodeContent child in new List<NodeContent>(node.Children))
			{
				ValidateMesh(child, context, parentBoneName);
			}
		}

		/// <summary>
		/// Checks whether a mesh contains skininng information.
		/// </summary>
		static bool MeshHasSkinning(MeshContent mesh)
		{
			foreach (GeometryContent geometry in mesh.Geometry)
			{
				if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Weights()))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Bakes unwanted transforms into the model geometry,
		/// so everything ends up in the same coordinate system.
		/// </summary>
		static void FlattenTransforms(NodeContent node, BoneContent skeleton)
		{
			foreach (NodeContent child in node.Children)
			{
				// Don't process the skeleton, because that is special.
				if (child == skeleton)
				{
					continue;
				}

				// Bake the local transform into the actual geometry.
				MeshHelper.TransformScene(child, child.Transform);

				// Having baked it, we can now set the local
				// coordinate system back to identity.
				child.Transform = Matrix.Identity;

				// Recurse.
				FlattenTransforms(child, skeleton);
			}
		}

		/// <summary>
		/// Force all the materials to use our skinned model effect.
		/// </summary>
		[DefaultValue(MaterialProcessorDefaultEffect.SkinnedEffect)]
		public override MaterialProcessorDefaultEffect DefaultEffect
		{
			get
			{
				return MaterialProcessorDefaultEffect.SkinnedEffect;
			}
			
			set
			{
			}
		}
	}
}