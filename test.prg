var a = 1;
var b = 1;

while (a <= 10)
{
    b = 1;

    if (a > 1) break;

    while (b <= 10)
    {
        print ("inner");
        print (b);
        b = b + 1;
        if (b > 2) { break; }
    }

    print ("outer");
    print (a);

    a = a + 1;
}

while(a > 20)
{
    print a;

    if (a == 10)
    {
        print("a is 10");
        break;
    }
    else
    {
        print("a is not 10");
    }

    a = a + 1;
}

print "bye";