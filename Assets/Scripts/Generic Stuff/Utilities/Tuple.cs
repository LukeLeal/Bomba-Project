using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tuple<T1, T2> {

    public T1 item1;
    public T2 item2;

    public Tuple (T1 t1, T2 t2) {
        this.item1 = t1;
        this.item2 = t2;
    }

}

// 3-tuple
public class Tuple<T1, T2, T3> {

    public T1 item1;
    public T2 item2;
    public T3 item3;

    public Tuple(T1 t1, T2 t2, T3 t3) {
        this.item1 = t1;
        this.item2 = t2;
        this.item3 = t3;
    }

}

