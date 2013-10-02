int* NewTest()
{
    return new int;
}

void DeleteTest(int *p)
{
    delete p;
}

int main()
{
    float *test = NewTest();
    DeleteTest(test);

    return 0;
}