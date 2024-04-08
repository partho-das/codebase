from cs50 import SQL

db = SQL("sqlite:///favorites.db")

favorite = input("Favorite: ")

rows = db.execute("select name from favorites limit ?", favorite)

for row in rows:
    print(row)

