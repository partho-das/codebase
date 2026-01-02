def main():
    name = input("What is your name: ").strip().title()

    """
    This is life
    """
    print("Hello, ", name, sep="")

# main()

# Ask user for their name
def print_hello(to):

    # Print Hello,
    print("Hello, ", end="")

    # Print Name
    print(to)
name = input("What is the name: ")


print_hello(name)