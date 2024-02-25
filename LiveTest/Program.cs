using HeapFile;

int test1 = 0;
int test2 = 15612;
int test3 = 48941;
long test4 = 48945654432;
string password = "Password";
string test5 = "Hey, what's up. This is an example string to test, if strings work!";
char[] test6 = ['H', 'e', 'l', 'l', 'o'];

/*if (File.Exists("File." + Fmem.FILE_EXTENSION))
{
    File.Delete("File." + Fmem.FILE_EXTENSION);
}*/

Fmem fmem = new Fmem("File." + Fmem.FILE_EXTENSION);

fmem.GetAllOldPointers(out Fpointer[] pointers);

Fpointer pointer;
Fpointer pointer2;
Fpointer pointer3;
Fpointer pointer4;
Fpointer passwPointer;
Fpointer pointer5;
Fpointer pointer6;

if (pointers.Length > 0)
{
    pointer = pointers[0];
    pointer2 = pointers[1];
    pointer3 = pointers[2];
    pointer4 = pointers[3];
    passwPointer = pointers[4];
    pointer5 = pointers[5];
    pointer6 = pointers[6];
}
else
{
    pointer = fmem.Alloc<int>(0);
    pointer2 = fmem.Alloc<int>(1);
    pointer3 = fmem.Alloc<int>(2);
    pointer4 = fmem.Alloc<long>(3);
    passwPointer = fmem.AllocString(ref password, 4);
    pointer5 = fmem.AllocString(ref test5, 5);
    pointer6 = fmem.Alloc<char>(test6.Length, 6);
}

fmem.Write<int>(pointer, test1);
fmem.Write<int>(pointer2, test2);
fmem.Write<int>(pointer3, test3);
fmem.Write<long>(pointer4, test4);
fmem.WriteString(passwPointer, password);
fmem.WriteString(pointer5, test5);
fmem.WriteArray<char>(pointer6, test6);

int read1 = fmem.Read<int>(pointer);
int read2 = fmem.Read<int>(pointer2);
int read3 = fmem.Read<int>(pointer3);
long read4 = fmem.Read<long>(pointer4);
string readPassw = fmem.ReadString(passwPointer);
string read5 = fmem.ReadString(pointer5);
char[] read6 = fmem.ReadArray<char>(pointer6, test6.Length);

Console.WriteLine(read1);
Console.WriteLine(read2);
Console.WriteLine(read3);
Console.WriteLine(read4);
Console.WriteLine(readPassw);
Console.WriteLine(read5);
Console.WriteLine(read6);
List<char> cleanPassword = new List<char>();
for (int i = 0; i < readPassw.Length; i++)
{
    cleanPassword.Add(' ');
}
fmem.WriteArray<char>(passwPointer, cleanPassword.ToArray());
readPassw = fmem.ReadString(passwPointer);
Console.WriteLine(readPassw);
fmem.Free(passwPointer);

string test7 = "NewStr";
Fpointer pointer7 = fmem.AllocString(ref test7, 7);
fmem.WriteString(pointer7, test7);
string read7 = fmem.ReadString(pointer7);
Console.WriteLine(read7);

int[] var = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100];
Fpointer pointer8 = fmem.Alloc<int>(var.Length, 9);
fmem.WriteArray<int>(pointer8, var);
fmem.Free(pointer8);
Console.WriteLine("Filesize: Before vs After:");
// Display old file size
Console.WriteLine(fmem.Length);
fmem.Shrink();
// Display new file size
Console.WriteLine(fmem.Length);

fmem.Close();

Console.ReadKey(true);