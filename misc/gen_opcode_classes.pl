use strict;
use warnings;
use v5.30;

mkdir 'tmp';

my %mathops = (
    add => 1,
    sub => 1,
    and => 1,
    or => 1,
    not => 1,
);

my %opcodes = (
    jalr => 1,
    in => 2,
    out => 3,
    addi => 4,
    shf => 5,
    lw => 6,
    sw => 7,
    lli => 8,
    lui => 9,
    beq => 10,
    bne => 11,
    bgt => 12,
    bge => 13,
    blt => 14,
    ble => 15,
    '.halt' => -1,
    '.fill' => -1,
    '.ralias' => -1,    
);

die "THIS SCRIPT IS OUT OF DATE.  Please see current definition of src/Instructions/InstructionDefinition.cs\n";

while( <DATA> ) {
    chomp;
    my( $mnemonic, $identifier, $mathop, $type, $opcount ) = split( /\t/ );
    my $adjm = substr( $mnemonic, 0, 1) eq '.' ? substr( $mnemonic, 1 ) : $mnemonic;

    my $name = ucfirst( $adjm ) . ( $type eq 'Directive' ? 'Directive' : 'Instruction' );
    my $nice_name = $name;

    my $opcode = $opcodes{$mnemonic} // 0;
    $mathop = 'NA' if $mathop eq '_Unused';

    mkdir 'src/Instructions/Definitions';

    open my $out, '>', "src/Instructions/Definitions/$name.cs";
    say $out <<"EOC";
using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class $name : AbstractInstruction
{
    public $name() : base()
    {
        Opcode = $opcode;
        Mnemonic = "$mnemonic";
        OperandCount = $opcount;
        Type = InstructionType.$type;
        MathOp = MathOperation.$mathop;
    }
}
EOC
    close $out;

    mkdir 'src/Assembler/InstructionHandlers';

    open $out, '>', "src/Assembler/InstructionHandlers/$name.cs";
    say $out <<"EOASM";
using AnnaSim.Assember;

namespace AnnaSim.Instructions.Definitions;

public partial class $name
{
    protected override void AssembleImpl(params Operand[] operands)
    {
        throw new NotImplementedException(\$"$name.{nameof(Assemble)}");
    }
}
EOASM
    close $out;

    mkdir 'src/Cpu/InstructionHandlers';

    open $out, '>', "src/Cpu/InstructionHandlers/$name.cs";
    say $out <<"EOCPU";
using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class $name
{
    protected override uint ExecuteImpl(Instruction instruction)
    {
        throw new NotImplementedException(\$"$name.{nameof(Execute)}");
    }
}
EOCPU
    close $out;
}
__DATA__
jalr	Jalr	_Unused	R	2
in	In	_Unused	R	1
out	Out	_Unused	R	1
addi	Addi	_Unused	Imm6	3
shf	Shf	_Unused	Imm6	3
lw	Lw	_Unused	Imm6	3
sw	Sw	_Unused	Imm6	3
lli	Lli	_Unused	Imm8	2
lui	Lui	_Unused	Imm8	2
beq	Beq	_Unused	Imm8	2
bne	Bne	_Unused	Imm8	2
bgt	Bgt	_Unused	Imm8	2
bge	Bge	_Unused	Imm8	2
blt	Blt	_Unused	Imm8	2
ble	Ble	_Unused	Imm8	2
add	_Math	Add	R	3
sub	_Math	Sub	R	3
and	_Math	And	R	3
or	_Math	Or	R	3
not	_Math	Not	R	2
.halt	_Halt	_Unused	Directive	0
.fill	_Fill	_Unused	Directive	-1
.ralias	_Ralias	_Unused	Directive	2
