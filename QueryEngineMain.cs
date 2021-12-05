using System.Collections.Generic;

namespace QueryEngineMain
{
    class Program
    {
        public static string InputQ = @"From users where ((FullName  = ""1 Mark Gil"" OR age > 13) AND fullname = ""bobi"") or age > 14  sELECT   age";
        public string InputQuery {get; set;}
    public class Data
    {
        public List<User> Users;
        public List<Order> Orders;
        // etc
    }
    
    public class User
    {
        public string Email {get; set;}
        public string FullName {get; set;}
        public int Age {get; set;}
        // etc
        public User(string email, string fullName, int age){
            Email = email;
            FullName = fullName;
            Age = age;
        }
        public override string ToString(){
            return $"Email: {Email}, Name: {FullName}, Age: {Age}";
        }
    }
    public class Order
    {
        public int ID;
        public string FullName;
        public int Sum;
        // etc
        public Order(int id, string fullName, int sum){
            ID = id;
            FullName = fullName;
            Sum = sum;
        }
    }
        public static void Main()
        {
            Data data = new Data();
            data.Users = new List<User>();
            data.Orders = new List<Order>();
            data.Users.Add(new User("mark1@ml.com", "1 Mark Gil", 11));
            data.Users.Add(new User("mark2@ml.com", "2 Mark Gil", 12));
            data.Users.Add(new User("mark3@ml.com", "3 Mark Gil", 13));
            data.Users.Add(new User("mark4@ml.com", "4 Mark Gil", 14));
            data.Users.Add(new User("mark5@ml.com", "5 Mark Gil", 15));
            data.Users.Add(new User("mark11@ml.com", "1 Mark Gil", 21));
            data.Users.Add(new User("mark21@ml.com", "2 Mark Gil", 32));
            data.Users.Add(new User("mark31@ml.com", "3 Mark Gil", 43));
            data.Users.Add(new User("mark41@ml.com", "4 Mark Gil", 54));
            data.Users.Add(new User("mark51@ml.com", "5 Mark Gil", 65));  
            data.Users.Add(new User("mark51@ml.com", "bar", 75));
            data.Users.Add(new User("mark51@ml.com", "foo", 85));
            myOwnParser myParser = new myOwnParser(InputQ);
            myParser.Tokenize(data);
        
        }
        
    }
}