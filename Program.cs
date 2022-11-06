using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace synt2
{
    internal class Program
    {
        public class Lexer
        {
            public enum State
            {
                Start, //
                Number_sign, // -+
                Number_char, // 0-9
                Int_notation,
                HexInt_notation,
                Int_char, // 0-9
                HexInt_char, //a-f, 0-9
                Float_dot, // .
                Float_char, // 0-9
                Float_e, // e
                Float_e_sign, // +-
                Float_e_power, // 0-9
                Char_start, // 0-255
                Char_number, // 0-255
                Literal_start, // '
                Literal_char, //  any char, no \n or ' 
                Literal_special,// '
                Literal_end,// '
                Identifier_start,// _,a-z
                Identifier_char,// _,a-z,0-9
                Comment_curly,// {
                Comment_double,// (*
                Comment_slash,// //
                Comment_curly_end,// }
                Comment_double_preend,// *
                Comment_double_end,// )
                Comment_slash_end,// \n
                Plus,// +
                Minus,// -
                Asterisk,// *
                Slash,// /
                Equal,// =
                Greater,// >
                Less,// <
                GreaterEq,// >=
                LessEq,// <=
                NotEq,// <>
                Colon,// :
                Becomes,// :=
                Semicolon,// ;
                Space,// ' '
                EOL,// \n
                EOF,// i = length
            }
            public enum Event
            {
                Number, // 0-9
                Character, // a-z
                HexNumber, // 0-9, a-f
                E, // 0-9
                Underscore,// _
                Quote, // '
                Curly_open,// {
                Bracket_open,// (
                Square_open,// [
                Curly_closed,// }
                Bracket_closed,// )
                Square_closed,// ]
                Plus,// +
                Minus,// -
                Asterisk,// *
                Slash,// /
                Equal,// =
                Greater,// >
                Less,// <
                Colon,// :
                Period,// .
                Hat,// ^
                Hash,// ^
                At,// @
                Dollar,// $
                Percent,// %
                Ampersand,// &
                Semicolon,// ;
                Space,// ' '
                EOL,// \n
                EOF,// i = length
            }
            public enum ASpecial
            {
                Plus,// +
                Minus,// -
                Asterisk,// *
                Slash,// /
                Equal,// =
                Greater,// >
                Less,// <
                GreaterEq,// >=
                LessEq,// <=
                NotEq,// <>
                Becomes,// :=
            }
            public enum DSpecial
            {
                Space,
                Semicolon,
                EOL,
                EOF,
            }
            public class IntData
            {
                private decimal value = 0;
                private int sign = 1;
                private int c_base = 10;
                public decimal Value
                {
                    get
                    {
                        return value * sign;
                    }
                }
                public int Sign
                {
                    get
                    {
                        return sign;
                    }
                    set
                    {
                        sign = value;
                    }
                }
                public void AddDigit(int a)
                {
                    value = value * c_base + a;
                }
                public void SetBase(int a)
                {
                    c_base = a;
                }
            }
            public class RealData
            {
                private double value = 0;
                private int sign = 1;
                private int afterDot = 0;
                private int expAddedValue = 0;
                private int expSign = 1;
                public double Value
                {
                    get
                    {
                        return value * sign;
                    }
                }
                public double ExpValue
                {
                    get
                    {
                        return (expAddedValue + 1) * expSign;
                    }
                }
                public int ExpSign
                {
                    get
                    {
                        return expSign;
                    }
                    set
                    {
                        expSign = value;
                    }
                }
                public int Sign
                {
                    get
                    {
                        return sign;
                    }
                    set
                    {
                        sign = value;
                    }
                }
                public void AddDigit(int a)
                {
                    value = value * 10 + a;
                }
                public void AddDecimal(int a)
                {
                    afterDot++;
                    value += Math.Pow(0.1, afterDot) * a;
                }
                public void AddExp(int a)
                {
                    expAddedValue += a;
                }
            }
            public class LiteralData
            {
                string value = "";
                static string ReplaceAtIndex(string s, char ch, int i)
                {
                    char[] t = s.ToCharArray();
                    t[i] = ch;
                    return string.Join("", t);
                }
                public void AddChar(char ch)
                {
                    value += ch;
                }
                public void IncreaceChar(int a)
                {
                    if (value[value.Length - 1]*10+a>255) throw new ArgumentException("expexted 0-255 int");
                    ReplaceAtIndex(value, (char)(value[value.Length - 1] + a), value.Length - 1);
                }
            }
            public class IdentifierData
            {
                string value = "";
                public void AddChar(char ch)
                {
                    value += ch;
                }
            }
            public class ASpecialData
            {
                public ASpecial Value { get; set; }
            }
            public class DSpecialData
            {
                public DSpecial Value { get; set; }
            }
            IntData intData;
            RealData realData;
            LiteralData literalData;
            IdentifierData identifierData;
            ASpecialData aSpecialData;
            DSpecialData dSpecialData;
            public Dictionary<State, Dictionary<Event, Action<char>>> lexerFSM;
            public HashSet<State> currentStates;
            string s;
            long it;
            long idx;
            long line;
            Lexer(string s)
            {
                this.s = s;
                it=0;
                idx=0;
                line=0;
                currentStates=new HashSet<State>();
                lexerFSM = new Dictionary<State, Dictionary<Event, Action<char>>>()
                {
                    [State.Start] = {
                        [Event.Number] = new Action<char>((ch)=>
                        {
                                currentStates.Add(State.Number_char);
                                intData.AddDigit(ch-'0');
                                realData.AddDigit(ch-'0');
                        }),
                        [Event.Plus] = new Action<char>((ch)=>
                        {
                                currentStates.Add(State.Plus);
                                currentStates.Add(State.Number_sign);
                                aSpecialData=ASpecial.Plus;
                                intData.Sign(1);
                                realData.Sign(1);
                        }),
                        [Event.Minus] = new Action<char>((ch)=>
                        {
                                currentStates.Add(State.Minus);
                                currentStates.Add(State.Number_sign);
                                aSpecialData=ASpecial.Minus;
                                intData.Sign(-1);
                                realData.Sign(-1);
                        }),
                        [Event.Character] = new Action<char>((ch)=>
                        {
                                identifierData.AddChar(ch);
                                currentStates.Add(State.Identifier_start);
                        }),
                        [Event.Underscore] = new Action<char>((ch)=>
                        {
                                identifierData.AddChar(ch);
                                currentStates.Add(State.Identifier_start);
                        }),
                        [Event.Asterisk] = new Action<char>((ch)=>
                        {
                                aSpecialData.Value=ASpecial.Asterisk;
                                currentStates.Add(State.Asterisk);
                        }),
                        [Event.Slash] = new Action<char>((ch)=>
                        {
                                aSpecialData.Value=ASpecial.Slash;
                                currentStates.Add(State.Slash);
                        }),
                        [Event.Equal] = new Action<char>((ch)=>
                        {
                                aSpecialData.Value=ASpecial.Equal;
                                currentStates.Add(State.Equal);
                        }),
                        [Event.Greater] = new Action<char>((ch)=>
                        {
                                aSpecialData.Value=ASpecial.Greater;
                                currentStates.Add(State.Greater);
                        }),
                        [Event.Less] = new Action<char>((ch)=>
                        {
                                aSpecialData.Value=ASpecial.Less;
                                currentStates.Add(State.Less);
                        }),
                        [Event.Colon] = new Action<char>((ch)=>
                        {
                                aSpecialData.Value=ASpecial.Colon;
                                currentStates.Add(State.Colon);
                        }),
                        [Event.Hat] = new Action<char>((ch)=>
                        {
                                aSpecialData.Value=ASpecial.Hat;
                                currentStates.Add(State.Hat);
                        }),
                        [Event.At] = new Action<char>((ch)=>
                        {
                                aSpecialData.Value=ASpecial.At;
                                currentStates.Add(State.At);
                        }),
                        [Event.Hash] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Char_start);
                            literalData.AddChar((char)0);
                        })
                        [Event.Dollar] = new Action<char>((ch)=>
                        {
                                currentStates.Add(State.HexInt_notation);
                                intData.c_base=16;
                        }),
                        [Event.Percent] = new Action<char>((ch)=>
                        {
                                currentStates.Add(State.Int_notation);
                                intData.c_base=2;
                        }),
                        [Event.Ampersand] = new Action<char>((ch)=>
                        {
                                currentStates.Add(State.Int_notation);
                                currentStates.Remove(State.Number_sign);
                                intData.c_base=8;
                        }),
                        [Event.Ampersand] = new Action<char>((ch)=>
                        {
                                currentStates.Add(State.Int_notation);
                                currentStates.Remove(State.Number_sign);
                                intData.c_base=8;
                        }),
                        [Event.Space] = new Action<char>((ch)=>
                        {
                                dSpecialData.Value=DSpecial.Space;
                                currentStates.Add(State.Space);
                        }),
                        [Event.Semicolon] = new Action<char>((ch)=>
                        {
                                dSpecialData.Value=DSpecial.Semicolon;
                                currentStates.Add(State.Semicolon);
                        }),
                        [Event.EOL] = new Action<char>((ch)=>
                        {
                                dSpecialData.Value=DSpecial.EOL;
                                currentStates.Add(State.EOL);
                        }),

                    },
                    [State.Number_sign] = {
                        [Event.Number] = new Action<char>((ch)=>
                        {
                                intData.AddDigit(ch-'0');
                                realData.AddDigit(ch-'0');
                        }),
                        [Event.Dollar] = new Action<char>((ch)=>
                        {
                                currentStates.Add(State.HexInt_notation);
                                currentStates.Remove(State.Number_sign);
                                intData.c_base=16;
                        }),
                        [Event.Percent] = new Action<char>((ch)=>
                        {
                                currentStates.Add(State.Int_notation);
                                currentStates.Remove(State.Number_sign);
                                intData.c_base=2;
                        }),
                        [Event.Ampersand] = new Action<char>((ch)=>
                        {
                                currentStates.Add(State.Int_notation);
                                currentStates.Remove(State.Number_sign);
                                intData.c_base=8;
                        }),
                    },
                    [State.Number_char] = {
                        [Event.Number] = new Action<char>((ch)=>
                        {
                                intData.AddDigit(ch-'0');
                                realData.AddDigit(ch-'0');
                        }),
                        [Event.Period] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Float_dot);
                            currentStates.Remove(State.Number_char);
                        }),
                    },
                    [State.Int_notation] =
                    {
                        [Event.Number] = new Action<char>((ch)=>
                        {
                                intData.AddDigit(ch-'0');
                                currentStates.Add(State.Int_char);
                                currentStates.Remove(State.Int_notation);
                        })
                    },
                    [State.Int_char] = {
                        [Event.Number] = new Action<char>((ch)=>
                        {
                                intData.AddDigit(ch-'0');
                        })
                    },
                    [State.HexInt_notation] =
                    {
                        [Event.Number] = new Action<char>((ch)=>
                        {
                                intData.AddDigit(ch-'0');
                                currentStates.Add(State.HexInt_char);
                                currentStates.Remove(State.HexInt_notation);
                        })
                    },
                    [State.HexInt_char] = {
                        [Event.HexNumber] = new Action<char>((ch)=>
                        {
                            if(ch>='0' && ch<='9')
                                intData.AddDigit(ch-'0');
                            if(ch>='a' && ch<='f')
                                intData.AddDigit(ch-'a');
                        })
                    },
                    [State.Float_char] = {
                        [Event.Number] = new Action<char>((ch)=>
                        {
                            realData.AddDecimal(ch-'0');
                        }),
                        [Event.E] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Float_e);
                            currentStates.Remove(State.Float_char);
                        })
                     },
                    [State.Float_e] = {
                        [Event.Plus] = new Action<char>((ch)=>
                        {
                            realData.ExpSign(1);
                            currentStates.Add(State.Float_e_sign);
                            currentStates.Remove(State.Float_e);
                        }),
                        [Event.Minus] = new Action<char>((ch)=>
                        {
                            realData.ExpSign(-1);
                            currentStates.Add(State.Float_e_sign);
                            currentStates.Remove(State.Float_e);
                        })
                    },
                    [State.Float_e_sign] = {
                        [Event.Number] = new Action<char>((ch)=>
                        {
                            realData.AddExp(ch-'0');
                            currentStates.Add(State.Float_e_power);
                            currentStates.Remove(State.Float_e_sign);
                        })
                    },
                    [State.Float_e_power] = {
                        [Event.Number] = new Action<char>((ch)=>
                        {
                            realData.AddExp(ch-'0');
                        })
                     },
                    [State.Char_start] = {
                        [Event.Number] = new Action<char>((ch)=>
                        {
                            literalData.IncreaceChar(ch-'0');
                            currentStates.Add(State.Char_number);
                            currentStates.Remove(State.Char_start);
                        })
                     },
                    [State.Char_number] = {
                        [Event.Number] = new Action<char>((ch)=>
                        {
                            literalData.IncreaceChar(ch-'0');
                        }),
                        [Event.Quote] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Literal_start);
                            currentStates.Remove(State.Char_number);
                        })
                     },
                    [State.Literal_start] = {
                        [Event.Quote] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Literal_end);
                            currentStates.Remove(State.Literal_start);
                        })
                        //runtime fill
                    },
                    [State.Literal_char] = {
                        [Event.Quote] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Literal_end);
                            currentStates.Remove(State.Literal_char);
                        })
                        //runtime fill
                    },
                    [State.Literal_special] = {
                        [Event.Quote] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Literal_end);
                            currentStates.Remove(State.Literal_special);
                        })
                        //runtime fill
                    },
                    [State.Literal_end] = {
                        [Event.Quote] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Literal_special);
                            currentStates.Remove(State.Literal_end);
                        })
                        [Event.Hash] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Char_start);
                            currentStates.Remove(State.Literal_end);
                            literalData.AddChar((char)0);
                        })
                    },
                    [State.Identifier_start] = {
                        [Event.Number] = new Action<char>((ch)=>
                        {
                            identifierData.AddChar(ch);
                        }),
                        [Event.Character] = new Action<char>((ch)=>
                        {
                            identifierData.AddChar(ch);
                        }),
                        [Event.Underscore] = new Action<char>((ch)=>
                        {
                            identifierData.AddChar(ch);
                        })
                    },
                    [State.Identifier_char] = {
                        [Event.Number] = new Action<char>((ch)=>
                        {
                            identifierData.AddChar(ch);
                        }),
                        [Event.Character] = new Action<char>((ch)=>
                        {
                            identifierData.AddChar(ch);
                        }),
                        [Event.Underscore] = new Action<char>((ch)=>
                        {
                            identifierData.AddChar(ch);
                        })
                    },
                    [State.Comment_curly] = {
                        [Event.Curly_closed] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Comment_curly_end);
                            currentStates.Remove(State.Comment_curly);
                        })
                        //runtime fill
                    },
                    [State.Comment_double] = {
                        [Event.Asterisk] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Comment_double_preend);
                            currentStates.Remove(State.Comment_double);
                        })
                        //runtime fill
                     },
                    [State.Comment_slash] = {
                        [Event.EOL] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Comment_slash_end);
                            currentStates.Remove(State.Comment_slash);
                        })
                        //runtime fill
                     },
                    [State.Comment_double_preend] = {
                        [Event.Bracket_closed] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Comment_double_end);
                            currentStates.Remove(State.Comment_double_preend);
                        })
                        //runtime fill
                     },
                    [State.Comment_curly_end] = { },
                    [State.Comment_double_end] = { },
                    [State.Comment_slash_end] = { },
                    [State.Plus] = { },
                    [State.Minus] = { },
                    [State.Asterisk] = { },
                    [State.Slash] = {
                        [Event.Slash] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Comment_slash);
                            currentStates.Remove(State.Slash);
                        })
                    },
                    [State.Greater] = {
                        [Event.Equal] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.GreaterEq);
                            currentStates.Remove(State.Greater);
                            aSpecialData.Value=ASpecial.GreaterEq;
                        })
                    },
                    [State.Less] = {
                        [Event.Equal] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.LessEq);
                            currentStates.Remove(State.Less);
                            aSpecialData.Value=ASpecial.LessEq;
                        }),
                        [Event.Greater] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.NotEq);
                            currentStates.Remove(State.Less);
                            aSpecialData.Value=ASpecial.NotEq;
                        })
                    },
                    [State.Colon] = {
                        [Event.Equal] = new Action<char>((ch)=>
                        {
                            currentStates.Add(State.Becomes);
                            currentStates.Remove(State.Colon);
                            aSpecialData.Value=ASpecial.Becomes;
                        })
                    },
                    [State.Equal] = { },
                    [State.GreaterEq] = { },
                    [State.LessEq] = { },
                    [State.NotEq] = { },
                    [State.Becomes] = { },
                    [State.Semicolon] = { },
                    [State.Space] = { },
                    [State.EOL] = { },
                };
                foreach (Event e in (Event[]) Enum.GetValues(typeof(State)))
                {
                        if(e!=Event.Asterisk)
                            lexerFSM[State.Comment_double][e] = new Action<char>((ch)=>{});
                        if(e!=Event.Curly_closed)
                            lexerFSM[State.Comment_curly][e]= new Action<char>((ch)=>{});
                        if(e!=Event.EOL)
                        {
                            lexerFSM[State.Comment_curly][e]= new Action<char>((ch)=>{});
                            if(e!=Event.Quote)
                            {
                                lexerFSM[State.Literal_char][e]= new Action<char>((ch)=>
                                {
                                    literalData.AddChar(ch);
                                });
                                lexerFSM[State.Literal_special][e]= new Action<char>((ch)=>
                                {
                                    currentStates.Add(State.Literal_char);
                                    currentStates.Remove(State.Literal_special);
                                    literalData.AddChar(ch);
                                });
                                lexerFSM[State.Literal_start][e]= new Action<char>((ch)=>
                                {
                                    currentStates.Add(State.Literal_char);
                                    currentStates.Remove(State.Literal_start);
                                    literalData.AddChar(ch);
                                });
                            }
                        }
                }
            }
            public void GetLex()
            {
                HashSet<Event> e;
                if(s[it]>='0' && s[it]<='9')
                {
                    e.Add(Event.Number);
                    e.Add(Event.HexNumber);
                }
                if((s[it]>='a' && s[it]<='z')||(s[it]>='A' && s[it]<='Z'))
                {
                    e.Add(Event.Character);
                    if((s[it]>='a' && s[it]<='z')||(s[it]>='A' && s[it]<='Z'))
                    {
                        e.Add(Event.HexNumber);
                    }
                    if(s[it]=='E' || s[it]=='e')
                    e.Add(Event.E);
                }
                switch (s[it]) {
                    case '_':
                        e.Add(Event.Underscore);
                        break;
                    case '{':
                        e.Add(Event.Curly_open);
                        break;
                    case '}':
                        e.Add(Event.Curly_closed);
                        break;
                    case '[':
                        e.Add(Event.Square_open);
                        break;
                    case ']':
                        e.Add(Event.Square_closed);
                        break;
                    case '(':
                        e.Add(Event.Bracket_open);
                        break;
                    case ')':
                        e.Add(Event.Bracket_closed);
                        break;
                    case '+':
                        e.Add(Event.Plus);
                        break;
                    case '-':
                        e.Add(Event.Minus);
                        break;
                    case '*':
                        e.Add(Event.Ampersand);
                        break;
                    case '/':
                        e.Add(Event.Slash);
                        break;
                    case '=':
                        e.Add(Event.Equal);
                        break;
                    case ':':
                        e.Add(Event.Colon);
                        break;
                    case '^':
                        e.Add(Event.Hat);
                        break;
                    case '@':
                        e.Add(Event.At);
                        break;
                    case '$':
                        e.Add(Event.Dollar);
                        break;
                    case '%':
                        e.Add(Event.Percent);
                        break;
                    case '&':
                        e.Add(Event.Ampersand);
                        break;
                    case '#':
                        e.Add(Event.Hash);
                        break;
                    case ' ':
                        e.Add(Event.Space);
                        break;
                    case ';':
                        e.Add(Event.Semicolon);
                        break;
                    case '\n':
                        e.Add(Event.EOL);
                        break;
                }
            }
        }
    }
    static void Main(string[] args)
    {
        foreach(var arg in args)
        {
            if(arg="-l")
            {
                
            }
        }
    }
}

