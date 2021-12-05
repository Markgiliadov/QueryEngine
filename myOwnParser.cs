using System;
using System.Collections.Generic;
using static QueryEngineMain.Program;

using System.Linq.Expressions;
using System.Linq.Dynamic;

using System.Linq.Dynamic.Core;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
public enum TokenType
{
    NotDefined,
    And,
    Or,
    OpenParenthesis,
    CloseParenthesis,
    Comma,
    Number,
    StringValue,
    PropertyValue,
    Operator,
    From,
    Where,
    Select,
    DataSource,
    User,
    Order,
    None,

}
class myOwnParser
{
    string[] myTokens {get; set;}
    string fromExpression {get; set;}
    string whereExpression {get; set;}
    string[] selectors {get; set;}
    string _dataSource {get; set;}
    int lengthOfTokenArr;

    private string chosenDataSource { get; set;}
    public myOwnParser(string sqlQuery){
        sqlQuery = Regex.Replace(sqlQuery, @"\s+", "");
        TokenType tokenType = TokenType.None;
        char cursorVal = ' ';
        int commaCounter = 0;
        int  cursorIndex = 0;
        string tokenStr = "";
        while(cursorIndex < sqlQuery.Length){
            cursorVal = sqlQuery.ElementAt(cursorIndex);
            tokenStr += cursorVal;
            switch (tokenType)
            {
                case TokenType.None:{
                    if(isFromToken(tokenStr)){
                        tokenType = TokenType.From;
                        tokenStr = "";
                    }
                }
                break;
                case TokenType.From:{
                    if(isDataSourceToken(tokenStr) != TokenType.None){
                        _dataSource = isDataSourceToken(tokenStr).ToString().ToLower();
                        tokenType = TokenType.DataSource;
                        tokenStr = "";
                    }
                }
                break;
                case TokenType.DataSource:{
                    if(isWhereToken(tokenStr)){
                        tokenType = TokenType.Where;
                        tokenStr = "";
                    }
                }
                break;
                case TokenType.Where:{
                    if(isSelectToken(tokenStr)){
                        tokenType = TokenType.Select;
                    }else{
                        this.whereExpression+=tokenStr;
                        if(tokenStr.Equals(","))
                            commaCounter++;
                        tokenStr = "";
                    }
                }
                    break;
                default:
                    throw new Exception("FAILED TO PARSE SQL STRING");
            }
            cursorIndex++;
        }
        string[] dels = {"select",","};
        selectors = this.whereExpression.ToLower().Split(dels, commaCounter + 2,
            StringSplitOptions.RemoveEmptyEntries);
    }
    private bool isSelectToken(string tokenStr){
        if(tokenStr.ToLower().Equals("select")
        )
            return true;
        else return false;
    }
    private bool isWhereToken(string tokenStr){
        if(tokenStr.ToLower().Equals("where"))
            return true;
        else return false;
    }
    private bool isFromToken(string tokenStr){
        if(tokenStr.ToLower().Equals("from"))
            return true;
        else return false;
    }
    private TokenType isDataSourceToken(string tokenStr){
        if(tokenStr.ToLower().Equals("users"))
            return TokenType.User;
        else if(tokenStr.ToLower().Equals("orders"))
            return TokenType.Order;
        else return TokenType.None;
    }
    private bool isProperty(string prop, PropertyInfo[] properties){
        foreach (var p in properties)
        {
            if(prop.ToLower().Equals(p.Name.ToLower()))
                return true;
                
        }
        return false;
    }
    private string isConstant(string constVal, string propName, PropertyInfo[] properties){
        if(constVal.Length >= 2 && (constVal.Contains('\'') || constVal.Contains('"'))){
            if(constVal.ElementAt(0).Equals('\'') && constVal.ElementAt(constVal.Length-1).Equals('\'') ||
            constVal.ElementAt(0).Equals('"') && constVal.ElementAt(constVal.Length-1).Equals('"') )
                constVal = constVal.Substring(1,constVal.Length-2);
        } 
            foreach (var p in properties){
                if((Regex.IsMatch(constVal, @"^\d+$")) && (propName.ToLower().Equals(p.Name.ToLower()))){
                    if(p.PropertyType == typeof(int)){
                        return constVal;
                    }
                } else
                if((propName.ToLower().Equals(p.Name.ToLower()))){
                    if(p.PropertyType == typeof(string)){
                        return constVal;
                    }
                }
            }
        
        return null;

    }
    private ExpressionType isBitWiseOperator(string bitWiseStr){
        if(bitWiseStr.ToLower().Equals("and"))
            return ExpressionType.And;
        else if(bitWiseStr.ToLower().Equals("or"))
            return ExpressionType.Or;
        return ExpressionType.Block;
    }
    private bool isOperator(string opStr){
        switch (opStr)
        {
            case "=":
                return true;
            case ">":
                return true;
            case "<":
                return true;
            case ">=":
                return true;
            case "<=":
                return true;
            case "<>":
                return true;
            default:
                return false;
        }
    }
    private Expression TryParsing(string expressions, List<User> users, Expression parameterExpression){
        expressions = Regex.Replace(expressions, @"\s+", "");
        int parenthasisCounter = 0;
        PropertyInfo[] userProps = typeof(User).GetProperties();
        char cursorVal = ' ';
        string tokenStr = "";
        TokenType tokenType = TokenType.None;
        string strLeftProp = "";
        string myOp = "";
        string constantVal = "";
        int stringSign = 0;
        string bitWiseOp = "";
        char cursorNextVal = ' ';
        string newExpr = "";

        for (int i = 0; i < expressions.Length; i++)
        {
            cursorVal = expressions.ElementAt(i);
            if(i + 1 != expressions.Length)
                cursorNextVal = expressions.ElementAt(i+1);
            else cursorNextVal = ' ';
            tokenStr+=cursorVal;
            newExpr+=cursorVal;
            if(cursorVal.Equals('(')){
                parenthasisCounter++;
                tokenType = TokenType.OpenParenthesis;
                tokenStr = "";
            }
            else if(cursorVal.Equals(')')){
                parenthasisCounter--;
                tokenType = TokenType.CloseParenthesis;
                tokenStr = "";
            }
                if(strLeftProp.Length == 0 && tokenType != TokenType.Operator){
                    if(isProperty(tokenStr,  userProps)){
                        tokenType = TokenType.PropertyValue;
                        strLeftProp = tokenStr;
                        tokenStr = "";
                    }
                }
                if(tokenType == TokenType.PropertyValue ){
                    if(!Char.IsSymbol(cursorNextVal) && isOperator(tokenStr)){
                        tokenType = TokenType.Operator;
                        myOp = tokenStr;
                        tokenStr = "";
                    }
                }
                if(cursorVal.Equals('\'') || cursorVal.Equals('"'))
                    stringSign++;
                if(tokenType == TokenType.Operator ){
                    if(stringSign == 2 && isConstant(tokenStr, strLeftProp, userProps) != null){
                        tokenType = TokenType.StringValue;
                        constantVal = isConstant(tokenStr, strLeftProp, userProps);
                        tokenStr = "";
                        stringSign = 0;
                    } else if( stringSign == 0 && isConstant(tokenStr, strLeftProp, userProps) != null && cursorNextVal != '\'' && cursorNextVal != '"' && (cursorNextVal  == ' ' || cursorNextVal == ')' || !Char.IsDigit(cursorNextVal))){
                        tokenType = TokenType.StringValue;
                        constantVal = isConstant(tokenStr, strLeftProp, userProps);
                        tokenStr = "";
                    }
                }  
                if(parenthasisCounter == 0 && i == expressions.Length -1 && tokenType == TokenType.CloseParenthesis){
                    return TryParsing(expressions.Substring(1,i-1), users, parameterExpression);
                }

                else if((tokenType == TokenType.StringValue || tokenType == TokenType.CloseParenthesis) && stringSign  %2 ==  0 && !isBitWiseOperator(tokenStr).Equals(ExpressionType.Block)){
                    if(tokenStr.ToLower().Equals("and") || tokenStr.ToLower().Equals("or") ){
                        if(tokenStr.ToLower().Equals("and")){
                            bitWiseOp = "and";
                            tokenType = TokenType.And;
                        }
                        else{
                            tokenType = TokenType.Or;
                            bitWiseOp = "or";
                        }
                        Expression leftNodes = null;
                        Expression rightNodes = null;
                        if(parenthasisCounter == 0){
                            if(newExpr.Contains('(') && newExpr.Contains(')')){
                                leftNodes = TryParsing(expressions.Substring(1,i-(bitWiseOp.Length+1)), users, parameterExpression);
                            } else leftNodes = TryParsing(expressions.Substring(0,i-(bitWiseOp.Length-1)), users, parameterExpression);
                                if(cursorNextVal.Equals('('))
                                    rightNodes = TryParsing(expressions.Substring(i+2, expressions.Length-i-2), users, parameterExpression);
                                else
                                    rightNodes = TryParsing(expressions.Substring(i+1, expressions.Length-i-1), users, parameterExpression);
                            if(rightNodes!=null){
                                return Expression.MakeBinary(isBitWiseOperator(tokenStr), leftNodes, rightNodes);
                            }
                        } 
                    }
                    tokenStr = "";
            }
        }
        return simpleExpressionParser(strLeftProp, myOp, constantVal, users, parameterExpression);
    }

    public void Tokenize(Data data){ 
        var parameterExpression = Expression.Parameter(typeof(User),"user");
        var parsedBinaryExpression = TryParsing(this.whereExpression, data.Users, parameterExpression);
        var finalLambda = Expression.Lambda<Func<User,bool>>(parsedBinaryExpression,parameterExpression);
        System.Console.WriteLine(finalLambda);

        var finalResEmails =data.Users.AsQueryable().Where(finalLambda).Select(user => user.Email);
        var finalResFullnames = data.Users.AsQueryable().Where(finalLambda).Select(user => user.FullName);
        var finalResAges = data.Users.AsQueryable().Where(finalLambda).Select(user => user.Age);
        var finalResEmailsAndAges =data.Users.AsQueryable().Where(finalLambda).Select(user => new{user.Email, user.Age});
        var finalResFullnamesAndAges = data.Users.AsQueryable().Where(finalLambda).Select(user => new{user.FullName, user.Age});
        var finalResEmailsAndFullNames = data.Users.AsQueryable().Where(finalLambda).Select(user => new{user.Email, user.FullName});
        var finalResAllData = data.Users.AsQueryable().Where(finalLambda).Select(user => user);

        bool isAgeSelector = false;
        bool isEmailSelector = false;
        bool isFullNameSelector = false;

        for(int i = 1;i<this.selectors.Length;i++)
        {
            string s = this.selectors[i];
            if(s.Equals("age"))
                isAgeSelector = true;
            else if(s.Equals("email"))
                isEmailSelector = true;
            else if(s.Equals("fullname"))
                isFullNameSelector = true;
            else
                throw new Exception("NON EXISTING SELECTORS");
        }
        if(isAgeSelector){
            if(isEmailSelector){
                    if(isFullNameSelector){
                        printResult(finalResAllData);
                } else printResult(finalResEmailsAndAges);
            } else if(isFullNameSelector)
                printResult(finalResFullnamesAndAges);
              else
                printResult(finalResAges);
        } else if(isEmailSelector){
            if(isFullNameSelector)
                printResult(finalResEmailsAndFullNames);
            else printResult(finalResEmails);
        } else if(isFullNameSelector) printResult(finalResFullnames);
    }
    private void printResult<T>(IQueryable<T> data){
        foreach (var val in data)
        {
            System.Console.WriteLine(val);
        }
    }
    private Expression comparisonOperatorConverter(string myOperator, Expression leftProp, Expression rightConst){
        switch (myOperator)
        {
            case "=":
                return Expression.Equal(leftProp, rightConst);
            case ">":
                return Expression.GreaterThan(leftProp, rightConst);
            case "<":
                return Expression.LessThan(leftProp, rightConst);
            case ">=":
                return Expression.GreaterThanOrEqual(leftProp, rightConst);
            case "<=":
                return Expression.LessThanOrEqual(leftProp, rightConst);
            case "<>":
                return Expression.NotEqual(leftProp, rightConst);
            default:
                throw new Exception("NO VALID OPERATOR");
        }
    }
    private Expression simpleExpressionParser(string leftPar, string myOperator, string rightPar, List<User> myQUsers, Expression parameterExpression){
        var propertyAge = Expression.Property(parameterExpression,"Age");
        var propertyEmail = Expression.Property(parameterExpression,"Email");
        var propertyFullName = Expression.Property(parameterExpression,"FullName");
        switch (leftPar.ToLower())
        {
            case "age":{
                var leftExprProperty = Expression.Property(parameterExpression,"Age");
                if(Regex.IsMatch(rightPar, @"^\d+$")){
                    int rightExprVal = int.Parse(rightPar); // if integer = parsed
                    var rightExpConstant = Expression.Constant(rightExprVal);
                    var expression = comparisonOperatorConverter(myOperator, leftExprProperty, rightExpConstant);
                    return expression;
                } else{
                    string rightExprVal = rightPar; // It's a string = i.e : Email
                    var rightExpConstant = Expression.Constant(rightExprVal);
                    var expression = comparisonOperatorConverter(myOperator, leftExprProperty, rightExpConstant);
                    return expression;  
               }
            }
                
            case "fullname":{
                var leftExprProperty = Expression.Property(parameterExpression,"FullName");
                if(Regex.IsMatch(rightPar, @"^\d+$")){
                    int rightExprVal = int.Parse(rightPar); // if integer = parsed
                    var rightExpConstant = Expression.Constant(rightExprVal);
                    var expression = comparisonOperatorConverter(myOperator, leftExprProperty, rightExpConstant);
                    return expression;
                } else{
                    string rightExprVal = rightPar; // It's a string = i.e : Email
                    var rightExpConstant = Expression.Constant(rightExprVal);
                    var expression = comparisonOperatorConverter(myOperator, leftExprProperty, rightExpConstant);
                    return expression;
               }
            }
               
            case "email":{
                var leftExprProperty = Expression.Property(parameterExpression,"Email");
                if(Regex.IsMatch(rightPar, @"^\d+$")){
                    int rightExprVal = int.Parse(rightPar); // if integer = parsed
                    var rightExpConstant = Expression.Constant(rightExprVal);
                    var expression = comparisonOperatorConverter(myOperator, leftExprProperty, rightExpConstant);
                    return expression;
                } else{
                    string rightExprVal = rightPar; // It's a string = i.e : Email
                    var rightExpConstant = Expression.Constant(rightExprVal);
                    var expression = comparisonOperatorConverter(myOperator, leftExprProperty, rightExpConstant);
                    return expression;
               }
            }
                    
            default:   
                throw new Exception("PARSING FAILED!");
        }
        
    }
}