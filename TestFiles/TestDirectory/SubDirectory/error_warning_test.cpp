int* NewTest()
{
    float un_used;
    return new int;
}

void DeleteTest(int *p)
{
    delete p;
}

int main()
{
    float *cast_error = NewTest();
    DeleteTest(cast_error);

    return 0;
}