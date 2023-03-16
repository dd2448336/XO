using System.Xml.Serialization;

namespace apiXO
{
    public static class PseudoGame
    {
        static string dbf = "dbf";
        public static void Initial()
        {
            if (!File.Exists(dbf))
            {
                Registration("user", "pas");
                Registration("user2", "pas");

                Serial();
            }
            DeSerial();
        }
        static Random rnd = new Random();
        static PtBD pbd = new PtBD();
        static XmlSerializer xmlSerializer = new XmlSerializer(typeof(PtBD));
        static List<Zayavka> zayavki = new List<Zayavka>();
        static void Serial()
        {
            using (StreamWriter fs = new StreamWriter(dbf))
            {
                xmlSerializer.Serialize(fs, pbd);
                fs.Close();
            }
        }
        static void DeSerial()
        {
            using (StreamReader fs = new StreamReader(dbf)) 
            {
                PtBD? xi = xmlSerializer.Deserialize(fs) as PtBD;
                if (xi != null)
                    pbd = xi;
                fs.Close();
            }
        }
        public static List<Sess> GetAll()
        {
            return pbd.Sess;
        }

        public static List<Zayavka> GetZayavki(int mus)
        {
            return zayavki.FindAll(t=>t.User1.UserID == mus|| t.User2.UserID == mus); 
        }

        public static List<Zayavka> SetZayavka(int mus, int tus)
        {
            Zayavka? zay = zayavki.Find(t => t.User1.UserID == mus && t.User2.UserID == tus);
            if (zay == null)
            {
                if (mus != tus)
                {
                    zay = new Zayavka(GetUserFromID(mus), GetUserFromID(tus));
                    zayavki.Add(zay);
                    zay.Id = zayavki.Count;
                }
            }
            return zayavki;
        }



        public static User? Registration(string name, string pass)
        {
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(pass))
            {
                if (!pbd.Users.Exists(t => t.Name == name))
                {
                    User us = new User(name, pass);
                    pbd.Users.Add(us);
                    us.UserID = pbd.Users.Count;
                    Serial();
                    return us;
                }
                return pbd.Users.Find(t => t.Name == name);
            }
            return null;
        }

        public static User? Autorization(string name, string pass)
        {
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(pass))
            {
                if (pbd.Users.Exists(t => t.Name == name))
                {
                    User? us = pbd.Users.Find(t => t.Name == name && t.Password == pass);
                    if (us != null)
                    {
                        us.OnLine = true;
                        Serial();
                        return us;
                    }
                }
            }
            return null;
        }

        public static UserPred LogOut(int id)
        {
            User? user1 = pbd.Users.Find(t => t.UserID == id);
            if (user1 != null)
            {
                Sess ses = GetUserSession(user1.UserID);
                if (ses.Error == null)
                user1.OnLine = false; ;

                Serial();
                return user1;
            }
            return new UserPred() { Error = "Указанный полдьзователь не найден" };
        }

        

        public static List<UserPred> GetUsers(bool online)
        {
            DeSerial();
            List<UserPred> users = new List<UserPred>();
            for (int i = 0; i < pbd.Users.Count; i++)
            {
                if (online)
                {
                    if (pbd.Users[i].OnLine)
                        users.Add(pbd.Users[i]);
                }
                else
                    users.Add(pbd.Users[i]);
            }
            return users;

        }

        public static Sess?  StartGS(int zayid)
                    {
            Zayavka?  zay = zayavki.Find(t=>t.Id == zayid);
            if (zay == null)
                return new Sess() { Error = "Заявка не обнаружена" };
            
            DeSerial();
            User? user1 = pbd.Users.Find(t => t.UserID == zay.User1.UserID && t.OnLine);
            User? user2 = pbd.Users.Find(t => t.UserID == zay.User2.UserID && t.OnLine);
            if (user1 != null && user2 != null)
            {
                if (pbd.Sess.Find(t => t.Igrok1.User.UserID == user1.UserID && t.IsRunning) == null && pbd.Sess.Find(t => t.Igrok2.User.UserID == user2.UserID && t.IsRunning) == null)
                {
                    Sess ses = new Sess(user1, user2);
                    user2.HasStep = true;
                    pbd.Sess.Add(ses);
                    ses.SessionID = pbd.Sess.Count;
                    ses.IsRunning = true;
                    Serial();
                    return ses;

                }
                return new Sess() { Error = "Указанный пользователь уже участвует" };
            }
            return new Sess() { Error = "Указанный пользователь не найден" };

        }

        public static Sess StopGS(Sess ses, User? result)
        {
            DeSerial();
            ses.IsRunning = false;
            ses.Result = result;
            Serial();
            return ses;
        }

        public static Sess AddStep(int us, int sesid, StepCoor stepcor)
        {
            UserPred? user = GetUserFromID(us); 
            if(user == null||  !user.HasStep)
                return new Sess() { Error = "Ход другого пользователя" };

            Sess? ses = pbd.Sess.Find(t => t.SessionID == sesid && (t.Igrok1.User.UserID == us || t.Igrok2.User.UserID == us) && t.IsRunning);
            if (ses != null)
            {
                if(ses.Steps.Exists( t=>t.Coordinate == stepcor) )
                    return new Sess() { Error = "Ячейка уже использована" };

                ses.Steps.Add(new Step() { UserID = us, StepType = ses.Igrok1.User.UserID == us ? ses.Igrok1.StepType : ses.Igrok2.StepType, Coordinate = stepcor });

                user.HasStep = false;
                UserPred us2 = GetAnotherUser(ses, user);
                us2.HasStep = true;


                ses = Analizer(ses);
                Serial();
                return ses;
            }
            else
            {
                return new Sess() { Error = "Сессия не запущена " };
            }

        }


        

        public static Sess GetSessionByTD(int sesID)
        {
            DeSerial();
            return pbd.Sess[sesID - 1];
        }
        public static Sess GetUserSession(int UsId)
        {
            DeSerial();
            Sess? ses = pbd.Sess.Find(t => (t.Igrok2.User.UserID == UsId || t.Igrok1.User.UserID == UsId) && t.IsRunning);
            if (ses != null)
                return ses;
            else
                return new Sess() { Error = "Запущенные сессии отсутствуют" };
        }



        public static Sess Analizer(Sess ses)
        {
            if (ses.GSBC(StepCoor.vl).Error==null&& ses.GSBC(StepCoor.vs).Error == null && ses.GSBC(StepCoor.vp).Error == null &&
                ses.GSBC(StepCoor.vl).StepType == ses.GSBC(StepCoor.vs).StepType && ses.GSBC(StepCoor.vl).StepType == ses.GSBC(StepCoor.vp).StepType)
                ses.Result = ses.Igrok1.StepType == ses.GSBC(StepCoor.vl).StepType ? pbd.Users.Find(t => t.UserID == ses.Igrok1.User.UserID) : pbd.Users.Find(t => t.UserID == ses.Igrok2.User.UserID);
            else if (ses.GSBC(StepCoor.sl).Error == null && ses.GSBC(StepCoor.ss).Error == null && ses.GSBC(StepCoor.sp).Error == null &&
                     ses.GSBC(StepCoor.sl).StepType == ses.GSBC(StepCoor.ss).StepType && ses.GSBC(StepCoor.sl).StepType == ses.GSBC(StepCoor.sp).StepType)
                ses.Result = ses.Igrok1.StepType == ses.GSBC(StepCoor.sl).StepType ? pbd.Users.Find(t => t.UserID == ses.Igrok1.User.UserID) : pbd.Users.Find(t => t.UserID == ses.Igrok2.User.UserID);
            else if (ses.GSBC(StepCoor.nl).Error == null && ses.GSBC(StepCoor.ns).Error == null && ses.GSBC(StepCoor.np).Error == null &&
                     ses.GSBC(StepCoor.nl).StepType == ses.GSBC(StepCoor.ns).StepType && ses.GSBC(StepCoor.nl).StepType == ses.GSBC(StepCoor.np).StepType)
                ses.Result = ses.Igrok1.StepType == ses.GSBC(StepCoor.nl).StepType ? pbd.Users.Find(t => t.UserID == ses.Igrok1.User.UserID) : pbd.Users.Find(t => t.UserID == ses.Igrok2.User.UserID);
            else


            if (ses.GSBC(StepCoor.vl).Error == null && ses.GSBC(StepCoor.sl).Error == null && ses.GSBC(StepCoor.nl).Error == null &&
                ses.GSBC(StepCoor.vl).StepType == ses.GSBC(StepCoor.sl).StepType && ses.GSBC(StepCoor.vl).StepType == ses.GSBC(StepCoor.nl).StepType)
                ses.Result = ses.Igrok1.StepType == ses.GSBC(StepCoor.vl).StepType ? pbd.Users.Find(t => t.UserID == ses.Igrok1.User.UserID) : pbd.Users.Find(t => t.UserID == ses.Igrok2.User.UserID);
            else if (ses.GSBC(StepCoor.vs).Error == null && ses.GSBC(StepCoor.ss).Error == null && ses.GSBC(StepCoor.ns).Error == null &&
                     ses.GSBC(StepCoor.vs).StepType == ses.GSBC(StepCoor.ss).StepType && ses.GSBC(StepCoor.vs).StepType == ses.GSBC(StepCoor.ns).StepType)
                ses.Result = ses.Igrok1.StepType == ses.GSBC(StepCoor.vs).StepType ? pbd.Users.Find(t => t.UserID == ses.Igrok1.User.UserID) : pbd.Users.Find(t => t.UserID == ses.Igrok2.User.UserID);
            else if (ses.GSBC(StepCoor.vp).Error == null && ses.GSBC(StepCoor.sp).Error == null && ses.GSBC(StepCoor.np).Error == null &&
                     ses.GSBC(StepCoor.vp).StepType == ses.GSBC(StepCoor.sp).StepType && ses.GSBC(StepCoor.vp).StepType == ses.GSBC(StepCoor.np).StepType)
                ses.Result = ses.Igrok1.StepType == ses.GSBC(StepCoor.vp).StepType ? pbd.Users.Find(t => t.UserID == ses.Igrok1.User.UserID) : pbd.Users.Find(t => t.UserID == ses.Igrok2.User.UserID);
            else

            if (ses.GSBC(StepCoor.vl).Error == null && ses.GSBC(StepCoor.ss).Error == null && ses.GSBC(StepCoor.np).Error == null &&
                ses.GSBC(StepCoor.vl).StepType == ses.GSBC(StepCoor.ss).StepType && ses.GSBC(StepCoor.vl).StepType == ses.GSBC(StepCoor.np).StepType)
                ses.Result = ses.Igrok1.StepType == ses.GSBC(StepCoor.vl).StepType ? pbd.Users.Find(t => t.UserID == ses.Igrok1.User.UserID) : pbd.Users.Find(t => t.UserID == ses.Igrok2.User.UserID);
            else if (ses.GSBC(StepCoor.vp).Error == null && ses.GSBC(StepCoor.ss).Error == null && ses.GSBC(StepCoor.nl).Error == null &&
                     ses.GSBC(StepCoor.vp).StepType == ses.GSBC(StepCoor.ss).StepType && ses.GSBC(StepCoor.vp).StepType == ses.GSBC(StepCoor.nl).StepType)
                ses.Result = ses.Igrok1.StepType == ses.GSBC(StepCoor.vp).StepType ? pbd.Users.Find(t => t.UserID == ses.Igrok1.User.UserID) : pbd.Users.Find(t => t.UserID == ses.Igrok2.User.UserID);
            
                if (ses.Steps.Count >= 9)
                {
                    ses.Result = null;
                    ses.IsRunning = false;
                }
                else
                if (ses.Result != null)
                {
                    ses.IsRunning = false;
                } 
            

            return ses;
        }

        public static int GetIdByName(string name)
        {
            int id = 0;
             UserPred? prd = pbd.Users.Find(t=>t.Name == name);
            if (prd != null)
                id = prd.UserID;
            return id;
        } 

        static User?  GetUserFromID(int uid)
        {
            return pbd.Users.Find(t => t.UserID == uid);    

        } 
        static UserPred GetAnotherUser(Sess ses, UserPred up)
        {
            UserPred? upp;

            if (ses.Igrok1.User.UserID == up.UserID)
                upp = pbd.Users.Find(t => t.UserID == ses.Igrok2.User.UserID);
            else
                upp = pbd.Users.Find(t => t.UserID == ses.Igrok1.User.UserID);
            if (upp == null)
            {
                upp = new UserPred();
                upp.Error = "Неизвестная ошибка;)";
            } 
            return upp;

        }

    }


} 



