import psycopg2  # Import the appropriate database library
C:\Users\Ceren.CEREN\PycharmProjects\postgreHelp\main.py
# Connect to your PostgreSQL database
conn = psycopg2.connect(
    database="Instagram",
    user="postgres",
    password="123",
    host="localhost",
    port="5432"
)

# Create a cursor
cur = conn.cursor()

# Loop through the range of filenames and execute UPDATE statements
for i in range(45, 46):
    filename = f'image{i-40}.jpeg'
    update_query = f"UPDATE users SET profile_pic = '{filename}' WHERE userId = {i};"
    cur.execute(update_query)

# Commit the changes and close the connection
conn.commit()
conn.close()
