# NDraw
A (probably) slowly evolving vector drawing tool.


# Windows Key Handling
How windows handles key presses. For example Shift+A produces:
- KeyDown: KeyCode=Keys.ShiftKey, KeyData=Keys.ShiftKey | Keys.Shift, Modifiers=Keys.Shift
- KeyDown: KeyCode=Keys.A, KeyData=Keys.A | Keys.Shift, Modifiers=Keys.Shift
- KeyPress: KeyChar='A'
- KeyUp: KeyCode=Keys.A
- KeyUp: KeyCode=Keys.ShiftKey
Also note that Windows steals TAB, RETURN, ESC, and arrow keys so they are not currently implemented.

# Windows Mouse Handling
From (https://docs.microsoft.com/en-us/dotnet/framework/winforms/mouse-events-in-windows-forms).

Mouse events occur in the following order:
- MouseEnter
- MouseMove
- MouseHover / MouseDown / MouseWheel
- MouseUp
- MouseLeave

MouseDown sequence looks like this:
- MouseDown
- Click
- MouseClick
- MouseUp
