import sys

try:
    x = int(input("Enter The Frist Nubmer: "))
    y = int(input("Enter The Second Number: "))
except ValueError:
    print("Error Caused for Wrong type")
    sys.exit(1)
try:
    z = x / y
except ZeroDivisionError:
    print("Error Caused by Divinding with Zero")
    sys.exit(1)
print(f"Answer is : {z}")
