# Multiplication program
#  From http://fac-staff.seattleu.edu/elarson/web/Research/fie08.pdf
#  Note some mnemonics have changed since this was published.

# Register usage:
# r1: constant one
# r2, r3: two numbers to multiply
# r4: final answer
# r5: stores sign of final answer
# (0-positive, nonzero - negative)
# store constant one into r1

        lli r1 1
        lui r1 1

        # get two input numbers
        in r2
        in r3
        
        # check for negative numbers
        bgt r2 &posA
        not r5 r5           # flip sign
        sub r2 r0 r2        # negate
posA:   bgt r3 &loop
        not r5 r5           # flip sign
        sub r3 r0 r3        # negate
        
        # main loop: while r3 > 0
loop:   beq r3 &done
        add r4 r4 r2        # add another r2 to r4
        sub r3 r3 r1        # decrement r3
        beq r0 &loop
        
        # output answer with proper sign
done:   beq r5 &disp
        sub r4 r0 r4        # negate
        
disp:   out r4
        .halt