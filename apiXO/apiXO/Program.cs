
using apiXO;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
PseudoGame.Initial();

app.MapGet("/reg", (string name, string pas) =>
{
    return PseudoGame.Registration(name, pas);
});

app.MapGet("/auth", (string name, string pas) =>
{
    return PseudoGame.Autorization(name, pas);
});  

app.MapGet("/gettusers", (string name, string pas, string online) =>
{
    List<UserPred> preds = new List<UserPred>();
    if (PseudoGame.Autorization(name, pas) != null)
        preds = PseudoGame.GetUsers(online == "1");
    else
        preds.Add(new UserPred() { Error = "Имя или пароль польователя неверны" });

    return preds;

});



app.MapGet("/zayavka", (string name, string pas, string tus) =>
{
    List<Zayavka>? zays = new List<Zayavka>();
    if (PseudoGame.Autorization(name, pas) != null)
        zays = PseudoGame.SetZayavka( PseudoGame.GetIdByName(name), Convert.ToInt32(tus));
    return zays;
});

app.MapGet("/zayavki", (string name, string pas) =>
{
    List<Zayavka>? zays = new List<Zayavka>();
    if (PseudoGame.Autorization(name, pas) != null)
        zays = PseudoGame.GetZayavki(PseudoGame.GetIdByName( name));
    return zays;
});


app.MapGet("/startgs", (string name, string pas, string zayid) =>
{
    Sess? ses = new Sess();
    if (PseudoGame.Autorization(name, pas) != null)
        ses = PseudoGame.StartGS( Convert.ToInt32(zayid));
    else
        ses.Error = "Имя или пароль польователя неверны";
    return ses;
});

app.MapGet("/step", (string name, string pas, string sesid, string stepcor) =>
{
    Sess? ses = new Sess();
    if (PseudoGame.Autorization(name, pas) != null)
        ses = PseudoGame.AddStep(PseudoGame.GetIdByName(name), Convert.ToInt32(sesid), (StepCoor)Convert.ToInt32(stepcor)); 
    else
        ses.Error = "Имя или пароль польователя неверны";
    return ses;
});

app.MapGet("/logout", (string name, string pas) =>
{
    UserPred? up = new UserPred();
    if (PseudoGame.Autorization(name, pas) != null)
        up = PseudoGame.LogOut(PseudoGame.GetIdByName(name));
    else
        up.Error = "Имя или пароль польователя неверны";
    return up;
});


app.MapGet("/session", (string name, string pas, string uid) =>
{
    Sess? sess = new Sess();
    if (PseudoGame.Autorization(name, pas) != null)
        sess = PseudoGame.GetUserSession(Convert.ToInt32(uid));
    else
        sess.Error = "Имя или пароль польователя неверны";
    return sess;
});

app.MapGet("/sessbid", (string name, string pas, string sid) =>
{
    Sess? sess = new Sess();
    if (PseudoGame.Autorization(name, pas) != null)
        sess = PseudoGame.GetSessionByTD(Convert.ToInt32(sid));
    else
        sess.Error = "Имя или пароль польователя неверны";
    return sess;
});

app.MapGet("/alses", (string name, string pas) =>
{
    List<Sess> sess = new List<Sess>();
    if (PseudoGame.Autorization(name, pas) != null)
        sess = PseudoGame.GetAll();
    else
        sess.Add(new Sess() { Error = "Имя или пароль польователя неверны" });
    return sess;
});

app.Run();
