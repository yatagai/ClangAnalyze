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
    int *test = NewTest();
    // DeleteTest(test);

    return 0;
}