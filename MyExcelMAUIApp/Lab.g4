grammar Lab;

compileUnit : logical  EOF ;

logical
    : NOT logical                          #NotExpr
    | arith op=(LE | GE | NE) arith        #CompareExpr
    | LPAREN logical RPAREN                #ParenLogicalExpr
    ;

arith
    : LPAREN arith RPAREN                                #ParenExpr
    | arith op=(MULTIPLY | DIVIDE | MOD | DIV) arith     #MulDivExpr
    | arith op=(ADD | SUBTRACT) arith                    #AddSubExpr
    | op=(MMAX | MMIN) LPAREN arglist RPAREN             #MinMaxExpr
    | NUMBER                                             #NumberExpr
    | IDENTIFIER                                         #CellRefExpr
    ;

arglist : arith (COMMA arith)* ;

// Lexer
MOD      : 'mod' ;
DIV      : 'div' ;
MMAX     : 'mmax' ;
MMIN     : 'mmin' ;
NOT      : 'not' ;
LE       : '<=' ;
GE       : '>=' ;
NE       : '<>' ;
ADD      : '+' ;
SUBTRACT : '-' ;
MULTIPLY : '*' ;
DIVIDE   : '/' ;
LPAREN   : '(' ;
RPAREN   : ')' ;
COMMA    : ',' ;
NUMBER     : [0-9]+ ;
IDENTIFIER : [A-Z]+ [1-9] [0-9]* ;
WS : [ \t\r\n]+ -> skip ;