# for item in zip(range(1, 4), ["partho", "protim", "das"], [6, 6, 3]):
#     print(type(item))
#     print(item)

value = 1234.5678

print(f"{value:10.2f}")  # Right-align within 10 characters, 2 decimal places
print(f"{value:<10.2f}") # Left-align within 10 characters, 2 decimal places
print(f"{value:^10.2f}") # Center-align within 10 characters, 2 decimal places
print(f"{value:+.2f}")   # Always show the sign, 2 decimal places
print(f"{value:,.2f}")   # Add comma as thousands separator, 2 decimal places
print(f"{value:.2e}")    # Scientific notation with 2 decimal places
print(f"{value:%}")      # Percentage format
