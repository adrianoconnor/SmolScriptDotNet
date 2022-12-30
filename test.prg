var start = ticks();

var a = 1;
var b = 1;

function pi()
{
    return (3.41);
    return 1;
}

function blah()
{
    print "blah 1";
    print "blah 2";
    print "blah 3";
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

print "pi = " + pi()*1;

print "CP1: " + (ticks() - start);

print "2 squared = " + (2 ** 2);

while (a <= 100000)
{
    while (b <= 10)
    {
        print ("inner");
        print (b);
        b = b + 4;
        if (b > 2) { break; }
    }

    //print ("outer");
    //print (a);

    a = a + 1;
}

print a;

print "CP2: " + (ticks() - start);

say("bye");

blah();

say_times("bye", 2);

print "Time taken (ticks): " + (ticks() - start);