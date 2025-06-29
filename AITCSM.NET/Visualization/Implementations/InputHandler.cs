using AITCSM.NET.Visualization.Abstractions;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace AITCSM.NET.Visualization.Implementations;

public class InputHandler : IInputHandler
{
    private readonly IInputContext _inputContext;
    private readonly IWindow _window;
    private readonly Action<IKeyboard, Key, int> _onKeyDown;
    private readonly Action<IMouse, System.Numerics.Vector2> _onMouseMove;
    public IKeyboard? PrimaryKeyboard { get; private set; }
    public IMouse? PrimaryMouse { get; private set; }

    public InputHandler(
        IInputContext inputContext,
        IWindow window,
        Action<IKeyboard, Key, int> onKeyDown,
        Action<IMouse, System.Numerics.Vector2> onMouseMove)
    {
        _inputContext = inputContext;
        _window = window;
        _onKeyDown = onKeyDown;
        _onMouseMove = onMouseMove;
    }

    public void RegisterInputEvents()
    {
        PrimaryKeyboard = _inputContext.Keyboards.FirstOrDefault();
        PrimaryMouse = _inputContext.Mice.FirstOrDefault();
        if (PrimaryKeyboard != null)
        {
            PrimaryKeyboard.KeyDown += _onKeyDown;
        }
        if (PrimaryMouse != null)
        {
            PrimaryMouse.Cursor.CursorMode = CursorMode.Raw;
            PrimaryMouse.MouseMove += _onMouseMove;
        }
    }
}