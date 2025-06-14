# CHIP-8 Emulator

A CHIP-8 emulator written in C# that accurately implements the classic 8-bit virtual machine originally designed in the 1970s for programming video games.

## Features

- ✅ Complete CHIP-8 instruction set implementation (35 opcodes)
- ✅ 64x32 pixel monochrome display
- ✅ 16-key hexadecimal keypad input
- ✅ Sound and delay timers
- ✅ Built-in font set for hexadecimal digits (0-F)
- ✅ Configurable frame rate with default 60 FPS
- ✅ Memory management with proper ROM loading
- ✅ Stack operations for subroutine calls
- ✅ Event-driven architecture for emulation lifecycle

## Architecture

The emulator follows a clean, modular design with separate concerns:

- **`Chip8`** - Main emulator class handling the fetch-decode-execute cycle
- **`Context`** - System state container (memory, registers, timers, display)
- **`DecodedInstruction`** - Instruction parsing and representation
- **`Input`** - Keyboard input handling using Win32 API
- **`BuiltInFonts`** - Default hexadecimal character sprites

## Memory Layout

```
0x000-0x1FF: System memory (font set storage)
0x200-0xFFF: Program memory (ROM loading area)
```

## Supported Instructions

The emulator implements all standard CHIP-8 instructions:

| Opcode | Mnemonic | Description |
|--------|----------|-------------|
| 00E0 | CLS | Clear display |
| 00EE | RET | Return from subroutine |
| 1NNN | JP addr | Jump to address |
| 2NNN | CALL addr | Call subroutine |
| 3XNN | SE Vx, byte | Skip if Vx = byte |
| 4XNN | SNE Vx, byte | Skip if Vx ≠ byte |
| 5XY0 | SE Vx, Vy | Skip if Vx = Vy |
| 6XNN | LD Vx, byte | Load byte into Vx |
| 7XNN | ADD Vx, byte | Add byte to Vx |
| 8XY0-8XYE | Various | Arithmetic and logic operations |
| 9XY0 | SNE Vx, Vy | Skip if Vx ≠ Vy |
| ANNN | LD I, addr | Load address into I |
| BNNN | JP V0, addr | Jump to V0 + address |
| CXNN | RND Vx, byte | Random byte AND byte |
| DXYN | DRW Vx, Vy, n | Draw sprite |
| EX9E/EXA1 | Key operations | Skip based on key state |
| FX07-FX65 | Various | Timer, memory, and register operations |

## Key Mapping

The CHIP-8 keypad is mapped to modern keyboard keys:

```
CHIP-8 Keypad:    Keyboard Mapping:
1 2 3 C           1 2 3 4
4 5 6 D    -->    Q W E R
7 8 9 E           A S D F
A 0 B F           Z X C V
```

## Usage

### Basic Implementation

```csharp
// Create emulator instance
var chip8 = new Chip8();

// Load ROM file
byte[] rom = File.ReadAllBytes("game.ch8");
chip8.Initialize(rom);

// Set up event handlers
chip8.OnEmulationRender += (context) => {
    // Render the framebuffer to your display
    RenderDisplay(context.Framebuffer);
};

chip8.OnEmulationUpdate += (context) => {
    // Update game state, handle input, etc.
};

// Start emulation
chip8.Start();

// Later, stop emulation
chip8.Stop();
```

### Configuration

```csharp
// Set custom frame rate (default is 60 FPS)
chip8.FPS = 120;

// Access emulation context
var context = chip8.Context;
Console.WriteLine($"PC: 0x{context.PC:X3}");
Console.WriteLine($"I: 0x{context.I:X3}");
```

## System Requirements

- .NET Framework/.NET Core
- Windows (for keyboard input via Win32 API)

## Technical Details

### Timers
- **Delay Timer**: Decrements at 60Hz when > 0
- **Sound Timer**: Decrements at 60Hz when > 0, triggers beep sound

### Display
- Resolution: 64x32 pixels
- Monochrome (0 = off, 1 = on)
- Sprites are XORed onto the display
- Collision detection sets VF register

### Memory
- 4KB total memory (4096 bytes)
- 16 general-purpose 8-bit registers (V0-VF)
- 16-level stack for subroutine calls
- Special registers: I (index), PC (program counter), SP (stack pointer)

## Error Handling

The emulator includes comprehensive error checking:
- Stack overflow/underflow detection
- Program counter bounds checking
- Invalid opcode handling
- ROM size validation

## Events

The emulator provides lifecycle events for integration:
- `OnEmulationStart` - Fired when emulation begins
- `OnEmulationUpdate` - Fired each cycle
- `OnEmulationRender` - Fired when display needs updating
- `OnEmulationStop` - Fired when emulation ends

## Contributing

Contributions are welcome! Please ensure:
- Code follows existing style conventions
- All CHIP-8 instructions remain accurately implemented
- Error handling is maintained
- Documentation is updated for new features
