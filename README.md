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

# Notes


https://devblogs.microsoft.com/dotnet/whats-new-in-windows-forms-runtime-in-net-5-0/
In.NET 5.0 we’ve lifted the bar higher and optimised several painting paths. Historically Windows Forms relied on GDI+ (and some GDI)
for rendering operations. Whilst GDI + is easier to use than GDI because it abstracts the device context(a structure with information
about a particular display device, such as a monitor or a printer) via the Graphics object, it’s also slow due to the additional overhead.
In a number of situations where we deal with solid colours and brushes, we have opted to use GDI.
We have also extended a number rendering - related APIs(e.g.PaintEventArgs) with IDeviceContext interface, which whilst may not be available
to Windows Forms developers directly, allow us to bypass the GDI+ Graphics object, and thus reduce allocations and gain speed. These optimisations
have shown a significant reduction in memory consumptions in redraw paths, in some cases saving x10 in memory allocations).


System.Text.Json library.
If you are starting from scratch and you want to use Json then go for Utf8Json which is generic and very fast. Also the startup costs are
lower than from JIL because Utf8Json and MessagePackSharp (same author) seem to hit a fine spot by having most code already put into the
main library and generate only a small amount of code around that.DataContractSerializer generates only the de/serialize code depending
on the code path you actually hit on the fly.
