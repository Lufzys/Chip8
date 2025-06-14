using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Chip8.Emulator.Emulation;

public class Chip8
{
    public Context Context;
    public bool IsRunning { get; private set; } = false;
    public bool IsStarted { get; private set; } = false;
    public int FPS { get; set; } = 60;
    public long FrameTime => 1000 / FPS;
    private Thread EmulationThread;
    
    public delegate void EmulationDelegate(Context context);
    public event EmulationDelegate OnEmulationStart;
    public event EmulationDelegate OnEmulationUpdate;
    public event EmulationDelegate OnEmulationRender;
    public event EmulationDelegate OnEmulationStop;
    private Random _random = new Random();

    public Chip8()
    {
        this.Context = new Context();
        Context.Initalize();
    }

    public void Initalize(byte[] rom)
    {
        if (rom == null)
            throw new ArgumentNullException(nameof(rom));

        if (rom.Length > this.Context.Memory.Length - 0x200)
            throw new ArgumentException("ROM size exceeds available memory space");

        Context.Initalize();
        IsRunning = false;
        IsStarted = false;
        this.Context.Load(BuiltInFonts.Default, 0x000); // Load default font set into system-reserved memory (0x000–0x1FF)
        this.Context.Load(rom, 0x200); // Load ROM into program memory starting at 0x200 (standard entry point for execution)
    }

    public void Start() // Start Cycle
    {
        if (!IsRunning)
        {
            IsRunning = true;
            EmulationThread = new Thread(new ThreadStart(Cycle));
            EmulationThread.Start();
        }
        else
            throw new Exception("Emulator already running!");
    }

    public void Stop() // Stop Cycle
    {
        if (IsRunning)
        {
            IsRunning = false;
            if (EmulationThread != null && EmulationThread.IsAlive)
            {
                EmulationThread.Join();
            }
        }
        else
            throw new Exception("Emulator already stopped!");
    }
    
    public ushort Fetch() // Fetch OpCodes
    {
        ushort opCode = (ushort)(this.Context[this.Context.PC] << 8 | this.Context[this.Context.PC + 1]); // Fetch OpCode
        Next(); // Skip next instructor
        return opCode; // return current OpCode
    }

    public DecodedInstruction Decode(ushort opCode) // Decode OpCodes
    {
        DecodedInstruction instruction = new DecodedInstruction();
        instruction.Parse(opCode);
        switch (instruction.Segment)
        {
            case 0x0:
                if (instruction.OpCode == 0x00E0)
                    return instruction.WithType(InstructionType.ClearScreen);
                else if  (instruction.OpCode == 0x00EE)
                    return instruction.WithType(InstructionType.Return);
                else
                    return instruction.WithType(InstructionType.CallMachineCode);
                break;
            case 0x1:
                return instruction.WithType(InstructionType.Jump);
                break;
            case 0x2:
                return instruction.WithType(InstructionType.CallSubroutine);
                break;
            case 0x3:
                return instruction.WithType(InstructionType.SkipIfEqualValue);
                break;
            case 0x4:
                return instruction.WithType(InstructionType.SkipIfNotEqualValue);
                break;
            case 0x5:
                return instruction.WithType(InstructionType.SkipIfEqualRegister);
                break;
            case 0x6:
                return instruction.WithType(InstructionType.LoadValue);
                break;
            case 0x7:
                return instruction.WithType(InstructionType.AddValue);
                break;
            case 0x8:
                switch (instruction.Nibble)
                {
                    case 0x0:
                        return instruction.WithType(InstructionType.LoadRegister);
                        break;
                    case 0x1:
                        return instruction.WithType(InstructionType.OrRegisters);
                        break;
                    case 0x2:
                        return instruction.WithType(InstructionType.AndRegisters);
                        break;
                    case 0x3:
                        return instruction.WithType(InstructionType.XorRegisters);
                        break;
                    case 0x4:
                        return instruction.WithType(InstructionType.AddRegisters);
                        break;
                    case 0x5:
                        return instruction.WithType(InstructionType.SubtractRegisters);
                        break;
                    case 0x6:
                        return instruction.WithType(InstructionType.ShiftRight);
                        break;
                    case 0x7:
                        return instruction.WithType(InstructionType.ReverseSubtract);
                        break;
                    case 0xE:
                        return instruction.WithType(InstructionType.ShiftLeft);
                        break;
                    default:
                        throw new ApplicationException($"Unknown OpCode: 0x{instruction.OpCode:X4} encountered at PC: 0x{this.Context.PC:X4}");
                }
                break;
            case 0x9:
                return instruction.WithType(InstructionType.SkipIfNotEqualRegister);
                break;
            case 0xA:
                return instruction.WithType(InstructionType.LoadIndex);
                break;
            case 0xB:
                return instruction.WithType(InstructionType.JumpPlusV0);
                break;
            case 0xC:
                return instruction.WithType(InstructionType.RandomAnd);
                break;
            case 0xD:
                return instruction.WithType(InstructionType.DrawSprite);
                break;
            case 0xE:
                switch (instruction.Value)
                {
                    case 0x9E:
                        return instruction.WithType(InstructionType.SkipIfKeyPressed);
                        break;
                    case 0xA1:
                        return instruction.WithType(InstructionType.SkipIfKeyNotPressed);
                        break;
                    default:
                        throw new ApplicationException($"Unknown OpCode: 0x{instruction.OpCode:X4} encountered at PC: 0x{this.Context.PC:X4}");
                }
                break;
            case 0xF:
                switch (instruction.Value)
                {
                    case 0x07:
                        return instruction.WithType(InstructionType.LoadDelayTimer);
                        break;
                    case 0x0A:
                        return instruction.WithType(InstructionType.WaitForKey);
                        break;
                    case 0x15:
                        return instruction.WithType(InstructionType.SetDelayTimer);
                        break;
                    case 0x18:
                        return instruction.WithType(InstructionType.SetSoundTimer);
                        break;
                    case 0x1E:
                        return instruction.WithType(InstructionType.AddToIndex);
                        break;
                    case 0x29:
                        return instruction.WithType(InstructionType.LoadSpriteLocation);
                        break;
                    case 0x33:
                        return instruction.WithType(InstructionType.StoreBCD);
                        break;
                    case 0x55:
                        return instruction.WithType(InstructionType.StoreRegisters);
                        break;
                    case 0x65:
                        return instruction.WithType(InstructionType.LoadRegisters);
                        break;
                }
                break;
            default:
                throw new ApplicationException($"Unknown OpCode: 0x{instruction.OpCode:X4} encountered at PC: 0x{this.Context.PC:X4}");
            break;
        }
        return instruction;
    }

    public void Execute(DecodedInstruction instruction) // Execute CPU command
    {
        Console.WriteLine($"Type: {instruction.Type}, OpCode: 0x{instruction.OpCode.ToString("X")}");
        switch (instruction.Type)
        {
            case InstructionType.ClearScreen:
                this.Context.Framebuffer = new int[64, 32];
                this.Context.DrawFlag = true;
                break;
            case InstructionType.Return:
                if (this.Context.SP == 0) // Stack underflow check
                    throw new InvalidOperationException("Stack underflow!");
                this.Context.PC = this.Context.Stack[this.Context.SP];
                this.Context.SP--;
                break;
            case InstructionType.Jump:
                this.Context.PC = (ushort)instruction.Address;
                break;
            case InstructionType.CallSubroutine:
                if (this.Context.SP >= 15) // Stack overflow check
                    throw new InvalidOperationException("Stack overflow!");
                this.Context.SP++;
                this.Context.Stack[this.Context.SP] = this.Context.PC;
                this.Context.PC = (ushort)instruction.Address;
                break;
            case InstructionType.SkipIfEqualValue:
                if (this.Context.V[instruction.X] == instruction.Value)
                    Next();
                break;
            case InstructionType.SkipIfNotEqualValue:
                if (this.Context.V[instruction.X] != instruction.Value)
                    Next();
                break;
            case InstructionType.SkipIfEqualRegister:
                if (this.Context.V[instruction.X] == this.Context.V[instruction.Y])
                    Next();
                break;
            case InstructionType.LoadValue:
                this.Context.V[instruction.X] = (byte)instruction.Value;
                break;
            case InstructionType.AddValue:
                this.Context.V[instruction.X] += (byte)instruction.Value;
                break;
            case InstructionType.LoadRegister:
                this.Context.V[instruction.X] = this.Context.V[instruction.Y];
                break;
            case InstructionType.OrRegisters:
                this.Context.V[instruction.X] |= this.Context.V[instruction.Y];
                break;
            case InstructionType.AndRegisters:
                this.Context.V[instruction.X] &= this.Context.V[instruction.Y];
                break;
            case InstructionType.XorRegisters:
                this.Context.V[instruction.X] ^= this.Context.V[instruction.Y];
                break;
            case InstructionType.AddRegisters:
                int result = this.Context.V[instruction.X] + this.Context.V[instruction.Y];
                this.Context.V[0xF] = (byte)(result > 255 ? 1 : 0); // Carry flag
                this.Context.V[instruction.X] = (byte)result;
                break;
            case InstructionType.SubtractRegisters:
                this.Context.V[0xF] = (byte)(this.Context.V[instruction.X] >= this.Context.V[instruction.Y] ? 1 : 0); // Borrow flag
                this.Context.V[instruction.X] -= this.Context.V[instruction.Y];
                break;
            case InstructionType.ShiftRight:
                this.Context.V[0xF] = (byte)(this.Context.V[instruction.X] & 0x1); // LSB
                this.Context.V[instruction.X] >>= 1;
                break;
            case InstructionType.ReverseSubtract:
                this.Context.V[0xF] = (byte)(this.Context.V[instruction.Y] >= this.Context.V[instruction.X] ? 1 : 0);
                this.Context.V[instruction.X] = (byte)(this.Context.V[instruction.Y] - this.Context.V[instruction.X]);
                break;
            case InstructionType.ShiftLeft:
                this.Context.V[0xF] = (byte)((this.Context.V[instruction.X] & 0x80) >> 7); // MSB
                this.Context.V[instruction.X] <<= 1;
                break;
            case InstructionType.SkipIfNotEqualRegister:
                if( this.Context.V[instruction.X] !=  this.Context.V[instruction.Y])
                    Next();
                break;
            case InstructionType.LoadIndex:
                this.Context.I = (ushort)instruction.Address;
                break;
            case InstructionType.JumpPlusV0:
                this.Context.PC = (ushort)(this.Context.V[0] + instruction.Address);
                break;
            case InstructionType.RandomAnd:
                this.Context.V[instruction.X] = (byte)(_random.Next(0, 256) & instruction.Value);
                break;
            case InstructionType.DrawSprite:
                int X = this.Context.V[instruction.X] % Context.ScreenWidth;
                int Y = this.Context.V[instruction.Y] % Context.ScreenHeight;
                int Width = 8;
                int Height = instruction.Nibble;
                
                this.Context.V[0xF] = 0; // Set VF to 0 to indicate no collision initially
                for (int row = 0; row < Height; row++)
                {
                    byte pixelData = this.Context.Memory[this.Context.I + row];
                    for (int col = 0; col < Width; col++)
                    {
                        if ((pixelData & (0x80 >> col)) != 0)
                        {
                            int screenX = (X + col) % Context.ScreenWidth;
                            int screenY = (Y + row) % Context.ScreenHeight;
                            
                            if (this.Context.Framebuffer[screenX, screenY] == 1) {
                                this.Context.V[0xF] = 1;  // Collision detected
                            }
                            this.Context.Framebuffer[screenX, screenY] ^= 1;
                        }
                    }
                }
                this.Context.DrawFlag = true;
                break;
            case InstructionType.SkipIfKeyPressed:
                if (this.Context.Keys[this.Context.V[instruction.X]])
                    Next();
                break;
            case InstructionType.SkipIfKeyNotPressed:
                if (!this.Context.Keys[this.Context.V[instruction.X]])
                    Next();
                break;
            case InstructionType.LoadDelayTimer:
                this.Context.V[instruction.X] = this.Context.DelayTimer;
                break;
            case InstructionType.WaitForKey:
                bool keyPressed = false;
                for (byte i = 0; i < 16; i++)
                {
                    if (this.Context.Keys[i])
                    {
                        this.Context.V[instruction.X] = i;
                        keyPressed = true;
                        break;
                    }
                }
                if (!keyPressed)
                    Restore();
                break;
            case InstructionType.SetDelayTimer:
                this.Context.DelayTimer = this.Context.V[instruction.X];
                break;
            case InstructionType.SetSoundTimer:
                this.Context.SoundTimer = this.Context.V[instruction.X];
                break;
            case InstructionType.AddToIndex:
                this.Context.I += this.Context.V[instruction.X];
                break;
            case InstructionType.LoadSpriteLocation:
                this.Context.I = (ushort)(0x000 + (this.Context.V[instruction.X] & 0xF) * 5); // system-reserved memory (0x000–0x1FF) 
                break;
            case InstructionType.StoreBCD:
                var number = this.Context.V[instruction.X];
                this.Context.Memory[this.Context.I] = (byte) (number / 100);
                this.Context.Memory[this.Context.I + 1] = (byte)((number / 10) % 10);
                this.Context.Memory[this.Context.I + 2] = (byte)(number % 10);
                break;
            case InstructionType.StoreRegisters:
                for (int i = 0; i <= instruction.X; i++)
                    this.Context.Memory[this.Context.I + i] = this.Context.V[i];
                break;
            case InstructionType.LoadRegisters:
                for (int i = 0; i <= instruction.X; i++)
                    this.Context.V[i] = this.Context.Memory[this.Context.I + i];
                break;
        }
    }

    public void Cycle() // Fetch>Decode>Execute Cycle
    {
        OnEmulationStart?.Invoke(this.Context);
        Stopwatch sw = new Stopwatch();
        while (IsRunning)
        {
            if (!IsStarted)
            {
                this.Context.PC = 0x200; // 0x200 => ROM EntryPoint
                IsStarted = true;
            }
            sw.Restart();
            Input.Update(this.Context);
            ushort OpCode = Fetch();
            DecodedInstruction instruction = Decode(OpCode);
            Execute(instruction);   
            OnEmulationUpdate?.Invoke(this.Context);
            if (this.Context.DrawFlag)
            {
                OnEmulationRender?.Invoke(this.Context);
                this.Context.DrawFlag = false;
            }
            if (this.Context.DelayTimer > 0) this.Context.DelayTimer--;
            if (this.Context.SoundTimer > 0) this.Context.SoundTimer--;
            long executeTime =  sw.ElapsedMilliseconds;
            long sleepTime = FrameTime - executeTime;
            if(sleepTime > 0)
                Thread.Sleep((int)sleepTime); // Sleep to maintain frame rate (~60Hz); consider SpinWait for more precision
            if (this.Context.SoundTimer > 0)
                Console.Beep(800, 100);
        }
        OnEmulationStop?.Invoke(this.Context);
    }
    
    public void Next() // Skip into next instructor
    {
        if (this.Context.PC >= this.Context.Memory.Length + Context.OpCodeStep)
            throw new InvalidOperationException("Program counter overflow!");
        this.Context.PC += Context.OpCodeStep; 
    }
    public void Restore() // Go back previous instructor
    {
        if (this.Context.PC - Context.OpCodeStep <= 0)
            throw new InvalidOperationException("Program counter underflow!");
        Context.PC -= Context.OpCodeStep;
    } 
}

public class Context
{
    public const int ScreenWidth = 64, ScreenHeight = 32;
    public const int OpCodeStep = 2;
    public int[,] Framebuffer = new int[ScreenWidth, ScreenHeight];
    public byte[] Memory { get; set; } = new byte[4096];
    public byte[] V { get; set; } = new byte[16];
    public ushort[] Stack { get; set; } = new ushort[16];
    public ushort I { get; set; }
    public ushort PC { get; set; }
    public ushort SP { get; set; }
    public byte DelayTimer { get; set; }
    public byte SoundTimer { get; set; }
    public bool DrawFlag { get; set; }
    public bool[] Keys = new bool[16];
    
    public Context() { }

    public byte this[int address] => this.Memory[address];
    
    public void Initalize()
    {
        Framebuffer = new int[ScreenWidth, ScreenHeight];
        Memory = new byte[4096];
        Stack = new ushort[16];
        I = 0;
        PC = 0;
        SP = 0;
        DelayTimer = 0;
        SoundTimer = 0;
        DrawFlag = false;
        Keys = new bool[16];
    }

    public void Load(byte[] buffer, int address) => Array.Copy(buffer, 0, Memory, address, buffer.Length);

    public byte Read(ushort address)
    {
        return this.Memory[address];
    }

    public void Write(ushort address, byte value)
    {
        this.Memory[address] = value;
    }
    
    public void ChangeKeyState(byte key, bool state) // BETTER NAME FOR FUNC
    {
        Keys[key] = state;
        DrawFlag = true;
    }
}

public class Input
{
    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(Keys vKey);

    public static Dictionary<Keys, int> KeyMap = new Dictionary<Keys, int>()
    {
        { Keys.X, 0x0 },
        { Keys.D1, 0x1 },
        { Keys.D2, 0x2 },
        { Keys.D3, 0x3 },
        { Keys.Q, 0x4 },
        { Keys.W, 0x5 },
        { Keys.E, 0x6 },
        { Keys.A, 0x7 },
        { Keys.S, 0x8 },
        { Keys.D, 0x9 },
        { Keys.Z, 0xA },
        { Keys.C, 0xB },
        { Keys.D4, 0xC },
        { Keys.R, 0xD },
        { Keys.F, 0xE },
        { Keys.V, 0xF }
    };

    public static void Update(Context context)
    {
        foreach (var pair in KeyMap)
        {
            context.Keys[pair.Value] = (GetAsyncKeyState(pair.Key) & 0x8000) != 0;
        }
    }
}

public struct DecodedInstruction
{
    public InstructionType Type { get; set; }
    public ushort OpCode { get; set; }
    public int Segment { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Nibble { get; set; }
    public int Value { get; set; }
    public int Address { get; set; }

    public void Parse(ushort opCode)
    {
        OpCode = opCode;
        Segment = OpCode >> 12;
        X = (OpCode & 0x0F00) >> 8;
        Y = (OpCode & 0x00F0) >> 4;
        Nibble = OpCode & 0x000F;
        Value = OpCode & 0x00FF;
        Address = OpCode & 0x0FFF;
    }

    public DecodedInstruction WithType(InstructionType type)
    {
        Type = type;
        return this;
    }
}

public enum InstructionType
{
    CallMachineCode,      // 0NNN Calls machine code routine at address NNN (RCA 1802)
    ClearScreen,          // 00E0 Clears the screen
    Return,               // 00EE Returns from a subroutine
    Jump,                 // 1NNN Jumps to address NNN
    CallSubroutine,       // 2NNN Calls subroutine at NNN
    SkipIfEqualValue,     // 3XNN Skips next instruction if VX == NN
    SkipIfNotEqualValue,  // 4XNN Skips next instruction if VX != NN
    SkipIfEqualRegister,  // 5XY0 Skips next instruction if VX == VY
    LoadValue,            // 6XNN Sets VX to NN
    AddValue,             // 7XNN Adds NN to VX (no carry flag)
    LoadRegister,         // 8XY0 Sets VX to VY
    OrRegisters,          // 8XY1 VX = VX OR VY
    AndRegisters,         // 8XY2 VX = VX AND VY
    XorRegisters,         // 8XY3 VX = VX XOR VY
    AddRegisters,         // 8XY4 VX += VY, set VF on carry
    SubtractRegisters,    // 8XY5 VX -= VY, set VF on borrow
    ShiftRight,           // 8XY6 VX >>= 1, VF = LSB before shift
    ReverseSubtract,      // 8XY7 VX = VY - VX, set VF on borrow
    ShiftLeft,            // 8XYE VX <<= 1, VF = MSB before shift
    SkipIfNotEqualRegister, // 9XY0 Skips if VX != VY
    LoadIndex,            // ANNN I = NNN
    JumpPlusV0,           // BNNN PC = V0 + NNN
    RandomAnd,            // CXNN VX = rand() & NN
    DrawSprite,           // DXYN Draw sprite at (VX, VY) height N
    SkipIfKeyPressed,     // EX9E Skip if key in VX pressed
    SkipIfKeyNotPressed,  // EXA1 Skip if key in VX not pressed
    LoadDelayTimer,       // FX07 VX = delay timer value
    WaitForKey,           // FX0A VX = key press (blocking)
    SetDelayTimer,        // FX15 delay timer = VX
    SetSoundTimer,        // FX18 sound timer = VX
    AddToIndex,           // FX1E I += VX
    LoadSpriteLocation,   // FX29 I = sprite location for digit VX
    StoreBCD,             // FX33 Store BCD of VX in I, I+1, I+2
    StoreRegisters,       // FX55 Store V0..VX in memory at I
    LoadRegisters         // FX65 Load V0..VX from memory at I
}

public class BuiltInFonts
{
    public static readonly byte[] Default =
    {
        0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
        0x20, 0x60, 0x20, 0x20, 0x70, // 1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
        0xF0, 0x80, 0xF0, 0x80, 0x80  // F
    };
}
