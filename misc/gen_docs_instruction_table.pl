use strict;
use warnings;
use v5.34;
use feature 'signatures';

my %COL_SPANS = (
    'Imm6' => 2,
    'Imm8' => 2,
);

my @i;
my $s = "";

foreach my $l ( <DATA> ) {
    if( $l =~ /^\d/ ) {
        push @i, $s if $s;
        $s = $l;
    } else {
        $s .= $l;
    }
}
push @i, $s;

open my $out, ">", "instruction_table.html";
css();
say $out "<table>";

foreach my $i ( @i ) {
    my @rows = split( /\n/sm, $i );

    my $bits = shift @rows;
    my $desc_line = shift @rows;
    my $docs = join( "<br/>", map { s/^\s+|\s+$//gr } @rows);
    $docs =~ s{^(<br/>)+}{}g;
    $docs =~ s{(<br/>)+$}{}g;
    
    my( $op, $desc ) = $desc_line =~ /^(\S+)\s+(.*)/;
    $desc = strip( $desc );

    $bits =~ s{Rs(\d)}{Rs<sub>$1</sub>}g;
    my @b = grep { $_ } map { strip( $_ ) } split( /\t/, $bits );
    my $bits_row = assemble_bits( @b );

    say "$op\t$desc";

    say $out "  <tr>";
    say $out "    <td class='op'><b>$op</b></td>";
    say $out "    <td>$desc</td>";
    say $out "    $bits_row";
    say $out "  </tr>";

    say $out "  <tr>";
    say $out "    <td colspan='18' class='docs'>$docs</td>";
    say $out "  </tr>";
}

say $out "</table>";

close $out;

sub strip {
    return $_[0] =~ s/^\s+|\s+$//gr;
}

sub assemble_bits {
    my @b = @_;
    my @pieces;
    push @pieces, "<td class='bits-box w4' colspan='4'>" . ( shift @b ) . '</td>';

    if( $b[$#b] eq 'Imm8' ) {
        push @pieces, join( '', 
            "<td class='bits-box w3' colspan='3'>$b[0]</td>",
            "<td class='bits-box w1' colspan='1'>x</td>",
            "<td class='bits-box w8' colspan='8'>Imm8</td>"
        );
    } elsif( $b[$#b] eq 'Imm6' ) {
        push @pieces, join( '', 
            "<td class='bits-box w3' colspan='3'>$b[0]</td>",
            "<td class='bits-box w3' colspan='3'>$b[1]</td>",
            "<td class='bits-box w6' colspan='6'>Imm6</td>"        
        );
    } else {
        push @pieces, join( '', 
            "<td class='bits-box w3' colspan='3'>$b[0]</td>",
            "<td class='bits-box w3' colspan='3'>$b[1]</td>",
            "<td class='bits-box w3' colspan='3'>$b[2]</td>",
            "<td class='bits-box w3' colspan='3'>$b[3]</td>"        
        );
    }

    return join( '', @pieces );
}

sub css {
    say $out <<'CSS';
<style>
    * {
        font-family: serif;
    }
    table {
        border-collapse: collapse;
        table-layout: fixed;
    }
    tbody {
        table-layout: fixed;
    }
    tr {
    }
    td {
        padding-left: 10px;
        padding-right: 10px;
    }
    .op {
        font-family: Courier, monospace;
        border: 1px solid;
        width: 3em;
    }
    .bits-box {
        border: 1px solid;
        font-family: sans-serif;
        text-align: center;
    }
    span.bit {
        width: 1em;
    }
    .docs {
        padding-top: 15px;
        padding-bottom: 25px;
    }
    .unused {
        font-size: 50%;
    }
    .op-bits {
    }
    td.w1 {
        width:1em;
    }
    td.w3 {
        width:3em;
    }
    td.w4 {
        width:4em;
    }
    td.w6 {
        width:6em;
    }
    td.w8 {
        width:8em;
    }
</style>
CSS
}
__DATA__
0 0 0 0 	Rd 	Rs1 	Rs2 	0 0 0 
add Add 
 
Two's complement addition.  Overflow is not detected. 
 
R(Rd) ← R(Rs1) + R(Rs2) 
 
 
0 0 0 0 	Rd 	Rs1 	Rs2 	0 0 1 
sub Subtract 
 
Two's complement subtraction. Overflow is not detected. 
 
R(Rd) ← R(Rs1) - R(Rs2) 
 
 
0 0 0 0 	Rd 	Rs1 	Rs2 	0 1 0 
and Bitwise and 
 
Bitwise and operation. 
 
R(Rd) ← R(Rs1) & R(Rs2) 
				
0 0 0 0 	Rd 	Rs1 	Rs2 	0 1 1 
or Bitwise or 

Bitwise or operation. 
 
R(Rd) ← R(Rs1) | R(Rs2) 
				
0 0 0 0 	Rd 	Rs1 	unused 	1 0 0 
not Bitwise not 
 
Bitwise not operation. 
 
R(Rd) ← ~R(Rs1) 

0 0 0 1 	Rd 	Rs1 	unused 	unused 
jalr Jump and link register 
 
Jumps to the address stored in register Rd and stores PC + 1 in register Rs1.  It is used for subroutine calls.  It can also be used for normal jumps by using register r0 as Rs1. 
 
R(Rs1) ← PC + 1 
PC ← R(Rd) 

0 0 1 0 	Rd 	unused 	unused 	unused 
in Get word from input 
 
Get a word from user input.  
 
R(Rd) ← input 

				
0 0 1 1 	Rd 	unused 	unused 	unused 
out Send word to output 
 
Send a word to output.  If Rd is r0, then the processor is halted. 
 
output ← R(Rd) 
 
 
0 1 0 0 	Rd 	Rs1 	Imm6 
addi Add immediate 
 
Two's complement addition with a signed immediate.  Overflow is not detected. 
 
R(Rd) ← R(Rs1) + Imm6 
 
 
0 1 0 1 	Rd 	Rs1 	Imm6 
shf Bit shift 
 
Bit shift.  It is either left if Imm6 is positive or right if the contents are negative.  The right shift is a logical shift with zero extension. 
 
if (Imm6 > 0) 
 	R(Rd) ← R(Rs1) << Imm6 else 
 	R(Rd) ← R(Rs1) >> Imm6 
 
0 1 1 0 	Rd 	Rs1 	Imm6 
lw 	Load word from memory
 
Loads word from memory using the effective address computed by adding Rs1 with the signed immediate.  
 
R(Rd) ← M[R(Rs1) + Imm6] 
 
0 1 1 1 	Rd 	Rs1 	Imm6 
sw 	Store word to memory 	
 
Stores word into memory using the effective address computed by adding Rs1 with the signed immediate. 
 
M[R(Rs1) + Imm6] ← R(Rd) 
 
 
1 0 0 0 	Rd 	 	Imm8 
lli Load lower immediate 
 
The lower bits (7-0) of Rd are copied from the immediate. The upper bits (15- 8) of Rd are set to bit 7 of the immediate to produce a sign-extended result.  
 
R(Rd[15..8]) ← Imm8[7] 
R(Rd[7..0]) ← Imm8 
 
 
1 0 0 1 	Rd 	 	Imm8 
lui Load upper immediate 
 
The upper bits (15- 8) of Rd are copied from the immediate. The lower bits (7-0) of Rd are unchanged. The sign of the immediate does not matter – the eight bits are copied directly. 
 
R(Rd[15..8]) ← Imm8 
 
1 0 1 0 	Rd 	 	Imm8 
beq Branch if equal to zero 
 
Conditional branch – compares Rd to zero.  If R(Rd) = 0, then branch is taken with indirect target of PC + 1 + Imm8 as next PC.  Immediate is a signed value. 
 
if (R(Rd) == 0) 	PC ← PC + 1 + Imm8 
 
1 0 1 0 	Rd 	 	Imm8 
bne Branch if not equal to zero 
 
Conditional branch – compares Rd to zero.  If R(Rd) ≠ 0, then branch is taken with indirect target of PC + 1 + Imm8 as next PC. Immediate is a signed value. 
 
if (R(Rd) ≠ 0) PC ← PC + 1 + Imm8 
 
 
1 1 0 0 	Rd 	 	Imm8 
bgt Branch if greater than zero 
 
Conditional branch – compares Rd to zero. If R(Rd) > 0, then branch is taken with indirect target of PC + 1 + Imm8 as next PC. Immediate is a signed value. 
 
if (R(Rd) > 0)  	PC ← PC + 1 + Imm8 

1 1 0 1 	Rd 	 	Imm8
bge 	Branch if greater than or equal to zero 	
 
Conditional branch – compares Rd to zero. If R(Rd) ≥ 0, then branch is taken with indirect target of PC + 1 + Imm8 as next PC. Immediate is a signed value. 
 
if (R(Rd) ≥ 0) PC ← PC + 1 + Imm8 
 
 
1 1 1 0 	Rd 	 	Imm8 
blt Branch if less than to zero 
 
Conditional branch – compares Rd to zero. If R(Rd) < 0, then branch is taken with indirect target of PC + 1 + Imm8 as next PC. Immediate is a signed value. 
 
if (R(Rd) < 0) PC ← PC + 1 + Imm8 
 
1 1 1 1 	Rd 	 	Imm8 
ble 	Branch if less than or equal to zero 	
 
Conditional branch – compares Rd to zero.  If R(Rd) ≤ 0, then branch is taken with indirect target of PC + 1 + Imm8 as next PC. Immediate is a signed value. 
 
if (R(Rd) ≤ 0)  	PC ← PC + 1 + Imm8 
