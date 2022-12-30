var a = 1;
var b = 1;

function blah()
{
    print "blah";
}

function say(what)
{
    print what;
}

function say_times(what, times)
{
    var a = 0;

    while (a < times)
    {
        print what;
        a = a + 1;
    }
}

print "2 squared = " + (2 ** 2);

var start = ticks();

while (a <= 10)
{
    while (b <= 10)
    {
        print ("inner");
        print (b);
        b = b + 4;
        if (b > 2) { break; }
    }

    print ("outer");
    print (a);

    a = a + 1;
}

print "Time taken (ticks): " + (ticks() - start);

say("bye");

blah();

say_times("bye", 2);
