import sqlite3
import json
import random
import string


db = sqlite3.connect('slon.db', check_same_thread=False)
c = db.cursor()

c.execute('''CREATE TABLE IF NOT EXISTS profiles (
    username TEXT,
    name TEXT,
    categories TEXT,
    tags TEXT,
    own_events TEXT,
    events TEXT,
    resume TEXT,
    swiped_users TEXT,
    mail TEXT
)''')
db.commit()

c.execute('''CREATE TABLE IF NOT EXISTS events (
    hash TEXT,
    name TEXT,
    categories TEXT,
    description TEXT,
    location TEXT,
    date_from INTEGER,
    date_to INTEGER
)''')
db.commit()

def find_profile(username: str):
    user = c.execute("SELECT username FROM profiles WHERE username = (?)", (username,)).fetchone()
    return user != None

def get_profile(username: str):
    user = c.execute("SELECT * FROM profiles WHERE username = (?)", (username,)).fetchone()
    return user

def add_profile(username: str, name="", categories="[]", tags="{}", own_events= "[]", events="[]", resume="", swiped_users="[]", mail=""):
    if not find_profile(username):
        c.execute("INSERT INTO profiles VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)", 
                  (username, name, categories, tags, own_events, events, resume, swiped_users, mail))
        db.commit()

def edit_profile_name(username: str, name: str):
    c.execute("UPDATE profiles SET name = ? WHERE username = ?", (name, username))
    db.commit()

def add_profile_category(username: str, category: str):
    if not find_profile(username):
        return
    
    categories_str = c.execute("SELECT categories FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    categories_list = json.loads(categories_str) if categories_str else []
    
    if category not in categories_list:
        categories_list.append(category)
    
    c.execute("UPDATE profiles SET categories = ? WHERE username = ?", (json.dumps(categories_list), username))
    db.commit()


def delete_profile_category(username: str, category: str):
    """Удаляет категорию из профиля."""
    if not find_profile(username):
        return
    
    categories_str = c.execute("SELECT categories FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    categories_list = json.loads(categories_str) if categories_str else []
    
    if category in categories_list:
        categories_list.remove(category)
    
    c.execute("UPDATE profiles SET categories = ? WHERE username = ?", (json.dumps(categories_list), username))
    db.commit()


def get_all_profiles():
    return c.execute("SELECT * FROM profiles").fetchall()


def add_profile_event(username: str, hash: string):
    if not find_profile(username):
        return
    
    events_str = c.execute("SELECT categories FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    events_list = json.loads(events_str) if events_str else []
    
    if hash not in events_list:
        events_list.append(hash)
    
    c.execute("UPDATE profiles SET categories = ? WHERE username = ?", (json.dumps(events_list), username))
    db.commit()


def delete_profile_event(username: str, hash: string):
    if not find_profile(username):
        return
    
    events_str = c.execute("SELECT events FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    events_list = json.loads(events_str) if events_str else []
    
    if id in events_list:
        events_list.remove(id)
    
    c.execute("UPDATE profiles SET events = ? WHERE username = ?", (json.dumps(events_list), username))
    db.commit()


def add_profile_own_event(username: str, hash: string):
    if not find_profile(username):
        return
    
    events_str = c.execute("SELECT own_events FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    events_list = json.loads(events_str) if events_str else []

    if id not in events_list:
        events_list.append(id)
    
    c.execute("UPDATE profiles SET own_events = ? WHERE username = ?", (json.dumps(events_list), username))
    db.commit()


def delete_profile_own_event(username: str, hash: string):
    if not find_profile(username):
        return
    
    events_str = c.execute("SELECT own_events FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    events_list = json.loads(events_str) if events_str else []
    
    if id in events_list:
        events_list.remove(id)
    
    c.execute("UPDATE profiles SET own_events = ? WHERE username = ?", (json.dumps(events_list), username))
    db.commit()


def edit_profile_resume(username: str, resume: str):
    c.execute("UPDATE profiles SET resume = ? WHERE username = ?", (resume, username))
    db.commit()


def add_profile_swiped_user(username: str, id: int):
    if not find_profile(username):
        return
    
    users_str = c.execute("SELECT swiped_users FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    users_list = json.loads(users_str) if users_str else []

    if id not in users_list:
        users_list.append(id)
    
    c.execute("UPDATE profiles SET swiped_users = ? WHERE username = ?", (json.dumps(users_list), username))
    db.commit()


def delete_profile_swiped_user(username: str, id: int):
    if not find_profile(username):
        return
    
    users_str = c.execute("SELECT swiped_users FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    users_list = json.loads(users_str) if users_str else []
    
    if id in users_list:
        users_list.remove(id)
    
    c.execute("UPDATE profiles SET swiped_users = ? WHERE username = ?", (json.dumps(users_list), username))
    db.commit()


def edit_profile_mail(username: str, mail: str):
    if not find_profile(username):
        return
    
    mails = c.execute("SELECT mail FROM profiles WHERE mail = ?", (mail,)).fetchone()

    if mails != None:
        return

    c.execute("UPDATE profiles SET mail = ? WHERE username = ?", (mail, username))
    db.commit()


def add_profile_tag(username: str, category: str, tag: str):
    if not find_profile(username):
        return
    
    tags_str = c.execute("SELECT tags FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    tags_dict = json.loads(tags_str) if tags_str else {}
    
    if category not in tags_dict:
        tags_dict[category] = []
    
    if tag not in tags_dict[category]:
        tags_dict[category].append(tag)
    
    c.execute("UPDATE profiles SET tags = ? WHERE username = ?", (json.dumps(tags_dict), username))
    db.commit()


def remove_profile_tag(username: str, category: str, tag: str):
    if not find_profile(username):
        return
    
    tags_str = c.execute("SELECT tags FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    tags_dict = json.loads(tags_str) if tags_str else {}
    
    if category not in tags_dict:
        return
    
    if tag in tags_dict[category]:
        tags_dict[category].remove(tag)
    
    if not tags_dict[category]:
        del tags_dict[category]
    
    c.execute("UPDATE profiles SET tags = ? WHERE username = ?", (json.dumps(tags_dict), username))
    db.commit()


def get_profile_tags(username: str):
    if not find_profile(username):
        return
    
    tags_str = c.execute("SELECT tags FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    tags_dict = json.loads(tags_str) if tags_str else {}
    
    return tags_dict


def is_email_exist(mail: string):
    mail = c.execute("SELECT mail FROM profiles WHERE mail = (?)", (mail,)).fetchone()
    return mail != None


def find_event(hash: string):
    event = c.execute("SELECT hash FROM events WHERE hash = (?)", (hash,)).fetchone()
    return event != None


def get_event(hash: str):
    event = c.execute("SELECT * FROM events WHERE hash = (?)", (hash,)).fetchone()
    return event



def generate_hash(length=6):
    characters = string.ascii_uppercase + string.digits
    hash_value = ''.join(random.choice(characters) for _ in range(length))
    
    return hash_value


# def add_event(name, categories, description, location, date_from, date_to, public, online):
#     hash = generate_hash()
#     while (find_event(hash)):
#         hash = generate_hash()

#     c.execute("INSERT INTO profiles VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)", 
#                 (hash, name, categories, description, location, date_from, date_to, public, online))
#     db.commit()

#     return hash


def edit_event_name(hash: str, name: str):
    c.execute("UPDATE events SET name = ? WHERE hash = ?", (name, hash))
    db.commit()


def add_event_category(hash: str, category: str):
    if not find_event(hash):
        return

    categories_str = c.execute("SELECT categories FROM events WHERE hash = ?", (hash,)).fetchone()[0]
    categories_list = json.loads(categories_str) if categories_str else []
    
    if category not in categories_list:
        categories_list.append(category)
    
    c.execute("UPDATE events SET categories = ? WHERE hash = ?", (json.dumps(categories_list), hash))
    db.commit()


def delete_event_category(hash: str, category: str):
    if not find_event(hash):
        return
    
    categories_str = c.execute("SELECT categories FROM events WHERE hash = ?", (hash,)).fetchone()[0]
    categories_list = json.loads(categories_str) if categories_str else []
    
    if category in categories_list:
        categories_list.remove(category)
    
    c.execute("UPDATE events SET categories = ? WHERE hash = ?", (json.dumps(categories_list), hash))
    db.commit()


def edit_event_description(hash: str, description: str):
    c.execute("UPDATE events SET description = ? WHERE hash = ?", (description, hash))
    db.commit()


def edit_event_location(hash: str, location: str):
    c.execute("UPDATE events SET location = ? WHERE hash = ?", (location, hash))
    db.commit()


def edit_event_date(hash: str, date_from: int, date_to: int):
    c.execute("UPDATE events SET date_from = ? WHERE hash = ?", (date_from, hash))
    c.execute("UPDATE events SET date_to = ? WHERE hash = ?", (date_to, hash))
    db.commit()
