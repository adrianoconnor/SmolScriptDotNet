class Parent2 {
    constructor() {
        console.log('Parent2 ctor');
    }

    moo() {
        console.log('parent moo2()');

    }

    beep() {
        console.log('beep');
    }

}

class Parent1 extends Parent2 {
    constructor() {
        super();
        this.y = 10;
        console.log('Parent1 ctor');
    }

    moo() {
        console.log('parent moo()  ---  ' + this.x);
    }

}

class Test extends Parent1 {
    constructor() {
        console.log('Test ctor');
        super()
        this.x = a;
    }

    moo() {
        console.log('moo()');
    }

    moof() {
        console.log('test moo() ' + this.y);
        super.moo();
        this.moo();
        super.beep();
        this.beep();
        // No: super.super.moo();
    }

}

let a = 9;

var t = new Test();

console.log(t);

t.moof();