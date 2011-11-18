using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Framework.Core.Helpers.Input
{
	///-------------------------------------------------------------------------------------   
	/// KeyboardBuffer.cs - provides buffered keyboard input XNA app via Win32 hooks   
	/// Author: Erin Hastings <ejhastings@gmail.com>   
	///    
	/// Based on code by Ben Ryves:   
	/// http://www.benryves.com/index.php?module=journal&mode=filtered&single_post=3024426   
	///    
	/// How to use:   
	///    
	/// (1) create a KeyboardBuffer object in your XNA game window class   
	///     ex: KeyboardBuffer kb = new KeyboardBuffer(Window.Handle);   
	///    
	/// (2) enable the buffer to capture key presses   
	///     ex: kb.Enable();   
	///    
	/// (3) add the captured in the buffer to a string   
	///     ex: string text = kb.GetText();   
	///        
	/// (4) make sure to disable the buffer when user is not typing   
	///     or the buffer may become extremely large (e.g. WASD movement keys)   
	///     ex: kb.disable()   
	///-------------------------------------------------------------------------------------   

	public class KeyboardBuffer : IDisposable
	{
		// the text buffer that user typed   
		// access contents with GetText()   
		private string buffer = "";

		// true if backspace key was pressed   
		// provide special handling outside the keybuffer class   
		private bool backSpaceKey;

		// true if enter key was pressed   
		// provide special handling outside the keybuffer class   
		private bool enterKey;

		// enable or disable buffer accumlation   
		// be sure to disable the buffer when user is not typing text   
		// or it will become very large (e.g. from WASD movement)   
		private bool enableBuffer;

		public bool IsShiftDown { get; private set; }
		public bool IsControlDown { get; private set; }

		public event KeyEventHandler KeyDown;
		public event KeyEventHandler KeyUp;
		public event KeyPressEventHandler KeyPress;

		///-------------------------------------------------------------------------------------   
		/// Return true if backspace was pressed.   
		/// Deleting chars must be handled outside the keybuffer class.   
		///-------------------------------------------------------------------------------------   
		public bool BackSpaceKey
		{
			get
			{
				bool b = backSpaceKey;
				backSpaceKey = false;
				return b;
			}
		}

		///-------------------------------------------------------------------------------------   
		/// Return true if enter was pressed.   
		/// Fire events from the enter key outside the keybuffer class,   
		/// e.g. when the user is done typing and buffer is submitted for use   
		///-------------------------------------------------------------------------------------   
		public bool EnterKey
		{
			get
			{
				bool b = enterKey;
				enterKey = false;
				return b;
			}
		}

		///-------------------------------------------------------------------------------------   
		/// Return true if buffer enabled.   
		///-------------------------------------------------------------------------------------   
		public bool Enabled
		{
			get
			{
				return enableBuffer;
			}
		}

		///-------------------------------------------------------------------------------------   
		/// Returns the buffer contents since last time GetText() was called.   
		///-------------------------------------------------------------------------------------   
		public string GetText()
		{
			string text = buffer;
			buffer = "";
			return text;
		}

		///-------------------------------------------------------------------------------------   
		/// <summary>   
		/// Returns text with spaces filtered out.   
		/// </summary>   
		///-------------------------------------------------------------------------------------   
		public string GetTextNoSpaces()
		{
			string text = buffer;
			buffer = "";
			if (text.Contains(" ")) text = "";
			return text;
		}

		///-------------------------------------------------------------------------------------   
		/// <summary>   
		/// Returns text only digits.   
		/// </summary>   
		///-------------------------------------------------------------------------------------   
		public string GetTextIntegersOnly()
		{
			string text = buffer;
			buffer = "";
			int i;
			bool numeric = Int32.TryParse(text, out i);
			if (numeric) return text;
			else return "";
		}

		///-------------------------------------------------------------------------------------   
		/// Clears the buffer.   
		///-------------------------------------------------------------------------------------   
		public void ClearBuffer()
		{
			buffer = "";
		}

		///-------------------------------------------------------------------------------------   
		/// Enables the buffering of keys.    
		/// Buffer should only be enabled when the user is typing text.   
		/// Do not leave buffer accumulation on during the entire game!   
		///-------------------------------------------------------------------------------------   
		public void Enable()
		{
			enableBuffer = true;
			buffer = "";
		}

		///-------------------------------------------------------------------------------------   
		/// Disables the buffering of keys   
		/// Buffer should be disabled when the user is not typing text!   
		///-------------------------------------------------------------------------------------   
		public void Disable()
		{
			enableBuffer = false;
			buffer = "";
		}

		#region Win32
		///-------------------------------------------------------------------------------------   
		/// Types of hook that can be installed using the SetWindwsHookEx function.   
		///-------------------------------------------------------------------------------------   
		public enum HookId
		{
			WH_CALLWNDPROC = 4,
			WH_CALLWNDPROCRET = 12,
			WH_CBT = 5,
			WH_DEBUG = 9,
			WH_FOREGROUNDIDLE = 11,
			WH_GETMESSAGE = 3,
			WH_HARDWARE = 8,
			WH_JOURNALPLAYBACK = 1,
			WH_JOURNALRECORD = 0,
			WH_KEYBOARD = 2,
			WH_KEYBOARD_LL = 13,
			WH_MAX = 11,
			WH_MAXHOOK = WH_MAX,
			WH_MIN = -1,
			WH_MINHOOK = WH_MIN,
			WH_MOUSE_LL = 14,
			WH_MSGFILTER = -1,
			WH_SHELL = 10,
			WH_SYSMSGFILTER = 6,
		};

		///-------------------------------------------------------------------------------------   
		/// A subset of relevant Windows message types.   
		///-------------------------------------------------------------------------------------   
		public enum WindowMessage
		{
			WM_KEYDOWN = 0x100,
			WM_KEYUP = 0x101,
			WM_CHAR = 0x102,
		};

		///-------------------------------------------------------------------------------------   
		/// A delegate used to create a hook callback.   
		///-------------------------------------------------------------------------------------   
		public delegate int GetMsgProc(int nCode, int wParam, ref Message msg);

		///-------------------------------------------------------------------------------------   
		/// Install an application-defined hook procedure into a hook chain.   
		/// If the function succeeds, the return value is the handle to the hook procedure.    
		/// Otherwise returns 0.   
		///   
		/// idHook : Specifies the type of hook procedure to be installed.   
		/// lpfn : Pointer to the hook procedure.   
		/// hmod : Handle to the DLL containing the hook procedure pointed to by the lpfn parameter.   
		/// dwThreadId : Specifies the identifier of the thread with which the hook procedure is   
		///               to be associated.   
		///-------------------------------------------------------------------------------------   
		[DllImport("user32.dll", EntryPoint = "SetWindowsHookExA")]
		public static extern IntPtr SetWindowsHookEx(HookId idHook, GetMsgProc lpfn, IntPtr hmod, int dwThreadId);

		///-------------------------------------------------------------------------------------   
		/// Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.    
		/// If the function fails, the return value is zero. To get extended error information,   
		/// call GetLastError.   
		///    
		/// hHook : Handle to the hook to be removed. This parameter is a hook handle obtained    
		///         by a previous call to SetWindowsHookEx.   
		///-------------------------------------------------------------------------------------   
		[DllImport("user32.dll")]
		public static extern int UnhookWindowsHookEx(IntPtr hHook);

		///-------------------------------------------------------------------------------------   
		/// Passes the hook information to the next hook procedure in the current hook chain.   
		/// Return : This value is returned by the next hook procedure in the chain.   
		///    
		/// hHook : Ignored.   
		/// ncode : Specifies the hook code passed to the current hook procedure.   
		/// wParam : Specifies the wParam value passed to the current hook procedure.   
		/// lParam : Specifies the lParam value passed to the current hook procedure.   
		///-------------------------------------------------------------------------------------   
		[DllImport("user32.dll")]
		public static extern int CallNextHookEx(int hHook, int ncode, int wParam, ref Message lParam);

		///-------------------------------------------------------------------------------------   
		/// Translates virtual-key messages into character messages.   
		/// If the message is translated (that is, a character message is posted to the thread's   
		/// message queue), the return value is true.   
		///    
		/// lpMsg : Pointer to an Message structure that contains message information retrieved    
		///         from the calling thread's message queue.   
		///-------------------------------------------------------------------------------------   
		[DllImport("user32.dll")]
		public static extern bool TranslateMessage(ref Message lpMsg);

		///-------------------------------------------------------------------------------------   
		/// <summary>   
		/// Retrieves the thread identifier of the calling thread.   
		/// </summary>   
		/// <returns>The thread identifier of the calling thread.</returns>   
		///-------------------------------------------------------------------------------------   
		[DllImport("kernel32.dll")]
		public static extern int GetCurrentThreadId();

		#endregion

		#region Hook management and class construction.
		///-------------------------------------------------------------------------------------   
		/// Handle for the created hook   
		//-------------------------------------------------------------------------------------   
		private readonly IntPtr HookHandle;
		private readonly GetMsgProc ProcessMessagesCallback;

		///-------------------------------------------------------------------------------------   
		/// Create an instance of the TextInputHandler.   
		/// Handle of the window you wish to receive messages (and thus keyboard input) from.   
		///-------------------------------------------------------------------------------------   
		public KeyboardBuffer(IntPtr whnd)
		{
			// create the delegate callback   
			// create the keyboard hook   
			this.ProcessMessagesCallback = new GetMsgProc(ProcessMessages);
			this.HookHandle = SetWindowsHookEx(HookId.WH_GETMESSAGE, this.ProcessMessagesCallback,
											   IntPtr.Zero, GetCurrentThreadId());
		}

		///-------------------------------------------------------------------------------------   
		/// Remove the keyboard hook.   
		///-------------------------------------------------------------------------------------   
		public void Dispose()
		{
			if (this.HookHandle != IntPtr.Zero) UnhookWindowsHookEx(this.HookHandle);
		}

		#endregion

		#region Message processing
		///-------------------------------------------------------------------------------------   
		/// Process Windows messages.   
		///-------------------------------------------------------------------------------------   
		private int ProcessMessages(int nCode, int wParam, ref Message msg)
		{
			// Check if we must process this message (and whether it has been retrieved via GetMessage):   
			if (nCode == 0 && wParam == 1)
			{
				// We need character input, so use TranslateMessage to generate WM_CHAR messages.   
				TranslateMessage(ref msg);

				// If it's one of the keyboard-related messages, raise an event for it:   
				switch ((WindowMessage)msg.Msg)
				{
					case WindowMessage.WM_CHAR:
						this.OnKeyPress(new KeyPressEventArgs((char)msg.WParam));
						break;
					case WindowMessage.WM_KEYDOWN:
						this.OnKeyDown(new KeyEventArgs((Keys)msg.WParam));
						break;
					case WindowMessage.WM_KEYUP:
						this.OnKeyUp(new KeyEventArgs((Keys)msg.WParam));
						break;
				}
			}

			// Call next hook in chain:   
			return CallNextHookEx(0, nCode, wParam, ref msg);
		}

		#endregion

		#region Events

		protected virtual void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyValue == 16)
			{
				IsShiftDown = true;
			}
			else if (e.KeyValue == 17)
			{
				IsControlDown = true;
			}

			if (KeyDown != null)
			{
				KeyDown(this, e);
			}
		}

		protected virtual void OnKeyUp(KeyEventArgs e)
		{
			if (e.KeyValue == 16)
			{
				IsShiftDown = false;
			}
			else if (e.KeyValue == 17)
			{
				IsControlDown = false;
			}

			if (KeyUp != null)
			{
				KeyUp(this, e);
			}
		}

		/// <summary>
		/// Only processes events if this buffer is enabled.   
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnKeyPress(KeyPressEventArgs e)
		{
			if (KeyPress != null)
			{
				KeyPress(this, e);
			}

			//if (enableBuffer)
			//{
			//    if (this.KeyPress != null) this.KeyPress(this, e);

			//    // if user pressed backspace   
			//    // set backspace to true   
			//    if (e.KeyChar.GetHashCode().ToString() == "524296")
			//    {
			//        backSpaceKey = true;
			//    }

			//    else if (e.KeyChar.GetHashCode().ToString() == "851981")
			//    {
			//        enterKey = true;
			//    }
			//    else
			//    {
			//        buffer += e.KeyChar;
			//    }
			//}
		}

		#endregion
	}  
}