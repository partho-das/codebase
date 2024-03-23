# class Point():
#     def __init__(self, x, y):
#         self.x = x
#         self.y = y


# p = Point(4, 5)

# print(p.x)
# print(p.y)

class Flight():
    def __init__(self, capacity):
        self.capacity = capacity
        self.passenger = []
    
    def open_seats(self):
        return self.capacity - len(self.passenger)
    def add_passenger(self, passenger):
        if not self.open_seats():
            return False
        else:
            self.passenger.append(passenger)
        return True

flight1 = Flight(2)
passengersList = ["partho", "faruk", "jhony", "gina"]

for people in passengersList:
    if flight1.add_passenger(people):
        print(f"{people} Added to the Flight")
    else:
        print(f"There is no room for {people}")
