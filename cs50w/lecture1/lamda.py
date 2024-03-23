def f(person):
    return person["house"]

people = [
    {"name": "harry", "house": "dhaka"},
    {"name": "faruk", "house": "azimpour"},
    {"name": "sazol", "house": "gopalgonz"},
    {"name": "samia", "house": "barishal"}
]

people.sort(key = lambda person: person['name'])
print(people)

