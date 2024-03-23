def announce(f):
    def warpper():
        print("About to Run the fucntion!")
        f()
        print("Done with the fucntion")
    return warpper

@announce
def hf():
    print("Hello World")

hf()