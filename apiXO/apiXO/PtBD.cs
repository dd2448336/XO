namespace apiXO
{
    public class Zayavka
    {

        public Zayavka(UserPred mus, UserPred tus)
        {
            User1 = mus;
            User2 = tus;
        } 
        public int Id { get; set; }
        public UserPred  User1 { set; get; }
        public UserPred User2 { set; get; } 

    } 

    [Serializable]
    public class PtBD
    {
        public PtBD()
        {
            Sess = new List<Sess>();
            Users = new List<User>();

        }
        public List<Sess> Sess { set; get; }
        public List<User> Users { set; get; }
    }

    public class UserPred
    {
        public int UserID { get; set; }
        public string? Name { get; set; }
        public bool OnLine { get; set; }
        public bool HasStep { set; get; }
        public string? Error { set; get; }
    }


    [Serializable]
    public class User : UserPred
    {
        public User()
        {

        }
        public User(string nam, string pas)
        {
            Name = nam;
            Password = pas;

        }
        public string? Password { get; set; }

    }

    [Serializable]
    public class Sess
    {
        public Sess()
        {

        }

        public Sess(User us1, User us2)
        {
            Igrok1.User = us1;
            Igrok2.User = us2;
        }
        public int SessionID { get; set; }
        public PodSet Igrok1 { set; get; } = new PodSet() { StepType = StepType.Krestik };
        public PodSet Igrok2 { set; get; } = new PodSet() { StepType = StepType.Nolik };
        public List<Step> Steps { get; set; } = new List<Step>();
        public bool IsRunning { get; set; }

        public UserPred? Result { set; get; } = null;

        public string? Error { set; get; }

        public Step GSBC(StepCoor stepcor)
        {
            Step? st = Steps.Find(t => t.Coordinate == stepcor);
            if (st == null)
                st = new Step() { Error = "пока пусто" };
            return st;
        }
    } 

    public class PodSet
    {
        public PodSet() { }

        public UserPred User { set; get; }
        public StepType StepType { set; get; }
    } 

    [Serializable]
    public class Step
    {
        public Step()
        {

        }
        public int UserID { set; get; }
        public StepType StepType { get; set; }
        public StepCoor Coordinate { set; get; }
        public string? Error { set; get; }
    }

    [Serializable]
    public enum StepCoor
    {
        vl = 1,
        vs = 2,
        vp = 3,
        sl = 4,
        ss = 5,
        sp = 6,
        nl = 7,
        ns = 8,
        np = 9
    }


    [Serializable]
    public enum StepType
    {
        Krestik,
        Nolik
    }
}
