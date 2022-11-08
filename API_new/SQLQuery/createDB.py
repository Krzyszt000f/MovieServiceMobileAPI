import psycopg2

sqlCreateTables = """
create table Users(
    user_guid VARCHAR(40) NOT NULL PRIMARY KEY,
    user_name VARCHAR(50) NOT NULL,
    email VARCHAR(50) UNIQUE NOT NULL,
    user_role VARCHAR(10) NOT NULL,
    refresh_token VARCHAR(150),
    token_expires TIMESTAMP,
    password_hash VARCHAR(100) NOT NULL,
    password_salt VARCHAR(200) NOT NULL
);

create table Movies(
    movie_guid VARCHAR(40) NOT NULL PRIMARY KEY,
    title VARCHAR(50) NOT NULL,
    date_of_production DATE NOT NULL,
    director VARCHAR(50) NOT NULL,
    actors VARCHAR(200),
    rating varchar(20),
    description varchar(1024)
);

create table Users_comments(
    comment_guid VARCHAR(40) NOT NULL PRIMARY KEY,
    user_guid VARCHAR(40) NOT NULL,
    movie_guid VARCHAR(40) NOT NULL,
    comment_content VARCHAR(50) NOT NULL,
    creation_date TIMESTAMP NOT NULL,
    FOREIGN KEY (user_guid) REFERENCES Users(user_guid),
    FOREIGN KEY (movie_guid) REFERENCES Movies(movie_guid)
);

create table Users_ratings(
    rating_guid VARCHAR(40) NOT NULL PRIMARY KEY,
    user_guid VARCHAR(40) NOT NULL,
    movie_guid VARCHAR(40) NOT NULL,
    rating VARCHAR(20) NOT NULL,
    FOREIGN KEY (user_guid) REFERENCES Users(user_guid),
    FOREIGN KEY (movie_guid) REFERENCES Movies(movie_guid)
);
"""

sqlInsertMovies = """
INSERT INTO Movies VALUES
    ('00000000-0000-0000-0000-000000000001', 'Star Wars: Episode IV – A New Hope', '1977-05-25', 'George Lucas', 'Mark Hamill, Harrison Ford, Carrie Fisher', '8.6/10', 'Luke Skywalker joins forces with a Jedi Knight, a cocky pilot, a Wookiee and two droids to save the galaxy from the Empires world-destroying battle station, while also attempting to rescue Princess Leia from the mysterious Darth Vader.'),
    ('00000000-0000-0000-0000-000000000002', 'The Godfather', '1972-03-14', 'Francis Ford Coppola', 'Marlon Brando, AlPacino, James Caan', '9.2/10', 'The aging patriarch of an organized crime dynasty in postwar New York City transfers control of his clandestine empire to his reluctant youngest son.'),
    ('00000000-0000-0000-0000-000000000003', 'The Devils Advocate', '1997-10-17', 'Taylor Hackford', 'Keanu Reeves, Al Pacino, Charlize Theron', '7.5/10', 'An exceptionally-adept Florida lawyer is offered a job at a high-end New York City law firm with a high-end boss--the biggest opportunity of his career to date.'),
    ('00000000-0000-0000-0000-000000000004', 'The Shawshank Redemption', '1994-09-13', 'Frank Darabont', 'Tim Robbins, Morgan Freeman, Bob Gunton', '9.3/10', 'Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.');
"""

sqlSelectMovies = """
SELECT * FROM Movies
"""

sqlSelectUsers = """
SELECT * FROM Users
"""

sqlInsertMovie = """
INSERT INTO Movies VALUES
('00000000-0000-0000-0000-000000000002', 'The Godfather', '1972-03-14', 'Francis Ford Coppola', 'Marlon Brando, AlPacino, James Caan', '9.2/10', 'The aging patriarch of an organized crime dynasty in postwar New York City transfers control of his clandestine empire to his reluctant youngest son.');
"""

sqlDelete = """
DELETE FROM Users;
"""

sqlAlterUsers = """
ALTER TABLE Users Add Column password_salt VARCHAR(200) NOT NULL;
"""

sqlDropUsers = """
ALTER TABLE Users DROP Column password_salt;
"""

#establishing the connection
conn = psycopg2.connect(
   database="movieservice", user='postgres', password='mysecretpassword', host='127.0.0.1', port= '5432'
)
conn.autocommit = True

#Creating a cursor object using the cursor() method
cursor = conn.cursor()

#Creating a database
#cursor.execute(sqlAlterUsers)

#------------------------------------------------------

#cursor.execute(sqladd)

print('\n'+sqlSelectUsers)
cursor.execute(sqlSelectUsers)
rows = cursor.fetchall()
for row in rows:
    print(row)

print("Done........")

#Closing the connection
conn.close()