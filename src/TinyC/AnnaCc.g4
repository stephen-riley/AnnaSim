grammar AnnaCc;

entrypoint  : program EOF;

program     : stat+ ;

stat        : var_decl ';'
            | assign ';'
            | func_proto
            | func_decl
            | flow_ctl
            | expr ';'
            ;

flow_ctl    : if_stat
            | while_stat
            | for_stat
            | return_stat
            | while_stat
            | do_while_stat
            ;

block       : '{' stat* '}' ;

var_decl    : simple_decl ( '=' ( a=atom | e=expr | al=array_literal ) )?
            ;

simple_decl : t=type name=ID ( arry='[' ( size=INT )? ']' )? ;

assign      : lval=lexpr '=' rhs=expr
            | ID opeq=OP_EQUAL opexpr=expr
            | ID op='++'
            | ID op='--'
            ;

func_signature
            : type name=ID '(' param_list ')'
            ;

func_proto  : func_signature ';' ;

func_decl   : func_signature body=block ;

if_stat     : 'if' '(' ifx=expr ')' ifblock=block 
              ( 'else' 'if' '(' elseifx+=expr ')' elseifblock+=block )* 
              ( 'else' elseblock=block )? 
            ;

while_stat  : 'while' '(' expr ')' block ;

do_while_stat
            : 'do' block 'while' expr ';' ;

for_stat    : 'for' '(' init=var_decl? ';' cond=expr ';' update=assign? ')' block ;

return_stat : 'return' expr ';' ;

func_call   : name=ID '(' args=arg_list ')' ;

param_list  : ( param+=simple_decl ( ',' param+=simple_decl )* )? ;

arg_list    : ( args+=expr ( ',' args+=expr )* )? ;

type        : 'char*' 
            | 'int*' 
            | 'char' 
            | 'int' 
            | 'void*' 
            | 'void'
            ;

expr        : op='+' unary=expr   // unary +
            | op='-' unary=expr   // unary -
            | op='*' unary=expr   // deref operator
            | lh=expr op = ( '&&' | '||' | '^' ) rh=expr
            | lh=expr op = ( '>=' | '<=' | '>' | '<' | '==' ) rh=expr
	          | lh=expr op = ( '*' | '/' | '%' ) rh=expr
	          | lh=expr op = ( '+' | '-' ) rh=expr
            | '(' inner=expr ')'
            | arryderef=lexpr '[' index=expr ']'
            | a=atom
            ;

array_literal
            : '{' aa+=atom  ( ',' aa+=atom )* '}'
            ;

atom        : sz=sizeof_atom
            | fn=func_call
            | ID
            | INT
            | CHAR
            | STRING 
            ;

sizeof_atom : SIZEOF '(' ( id=ID | t=type ) ')' ;

lexpr       : op='+' unary=lexpr
            | op='-' unary=lexpr
            | op='*' unary=lexpr
	          | lh=lexpr op=( '*' | '/' | '%' ) rh=lexpr
	          | lh=lexpr op=( '+' | '-' ) rh=lexpr
            | '(' inner=lexpr ')'
            | arryderef=lexpr '[' index=expr ']'
            | a=latom
            ;

latom       : ID
            | INT
            | func_call
            ;


// lexer

INT         : '0x' [0-9a-fA-F]+
            | '0b' [0-1]+
            | [0-9]+
            ;

SIZEOF      : 'sizeof' ;

CHAR        : '\'' . '\'' ;

STRING      : '"' .*? '"' ;

OP_EQUAL    : ( '+' | '-' | '*' | '/' ) '=';

ID          : [a-zA-Z_][a-zA-Z0-9_]* ;

LCOMMENT    : '//' ~[\r\n]* -> skip ;
WS          : [ \r\n\t]+ -> skip ;