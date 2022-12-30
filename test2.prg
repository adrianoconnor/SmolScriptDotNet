function test1()
{
    print 1;

    while(1 == 1)
    {
        print 2;

        while(1 == 1)
        {
            print 3;

            return 2;

            break;
        }

        print 4;

        return 1;
    }

    print 5;
}

function test2()
{
    print 1;
    while(1 == 1)
    {
        print 2;
        return 1;
        break;
    }
    print 3;
    return 2;
}

print test1();
