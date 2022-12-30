var a = 1;
var b = 1;

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

print "Time taken: (ticks)";
print (ticks() - start);

print "bye";