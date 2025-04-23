import sqlite3
import json
import random
import string


db = sqlite3.connect('slon.db', check_same_thread=False)
c = db.cursor()
c.execute('''CREATE TABLE IF NOT EXISTS profiles (
    username TEXT,
    name TEXT,
    surname TEXT,
    categories TEXT,
    tags TEXT,
    skills TEXT,
    events TEXT,
    description TEXT,
    swiped_users TEXT,
    swiped_events TEXT,
    mail TEXT,
    vocation TEXT
)''')
db.commit()

c.execute('''CREATE TABLE IF NOT EXISTS events (
    hash TEXT,
    owner TEXT,
    members TEXT,
    name TEXT,
    categories TEXT,
    description TEXT,
    location TEXT,
    date_from TEXT,
    date_to TEXT,
    public INTEGER,
    online INTEGER
)''')
db.commit()

c.execute('''CREATE TABLE IF NOT EXISTS users_passwords (
    username TEXT,
    password TEXT
)''')
db.commit()

c.execute('''CREATE TABLE IF NOT EXISTS notifications (
    username TEXT,
    username_from TEXT,
    is_request_or_accepted BOOLEAN /* 0 - request, 1 - accepted */
)''')
db.commit()

c.execute('''CREATE TABLE IF NOT EXISTS mutual (
    pair TEXT   /* в виде username1,username2 */
)''')
db.commit()



def create_notification(username: str, username_from: str):
    existing = c.execute(
        "SELECT 1 FROM notifications WHERE username = ? AND username_from = ? AND is_request_or_accepted = 0",
        (username, username_from)
    ).fetchone()

    if not existing:
        c.execute("INSERT INTO notifications VALUES (?, ?, ?)", (username, username_from, 0))
        db.commit()


def delete_notification(username: str, username_from: str):
    c.execute("DELETE FROM notifications WHERE username = ? AND username_from = ?", (username, username_from))
    db.commit()

def get_requests(username: str):
    data = c.execute("SELECT * FROM notifications WHERE username = ? AND is_request_or_accepted = 0", (username,)).fetchall()
    
    result = []
    for req in data:
        result.append(req[1])

    return result

def create_accepted_request(username: str, username_from: str):
    existing = c.execute(
        "SELECT 1 FROM notifications WHERE username = ? AND username_from = ? AND is_request_or_accepted = 1",
        (username, username_from)
    ).fetchone()

    if not existing:
        c.execute("INSERT INTO notifications VALUES (?, ?, ?)", (username, username_from, 1))
        db.commit()


def delete_accepted_request(username: str, username_from: str):
    c.execute("DELETE FROM notifications WHERE username = ? AND username_from = ?", (username, username_from))
    db.commit()

def get_accepted(username: str):
    data = c.execute("SELECT * FROM notifications WHERE username = ? AND is_request_or_accepted = 1", (username,)).fetchall()
    return [acc[1] for acc in data]

def add_mutual_user(username_one: str, username_two: str):
    pair1 = f"{username_one},{username_two}"
    pair2 = f"{username_two},{username_one}"

    existing = c.execute("SELECT 1 FROM mutual WHERE pair = ? OR pair = ?", (pair1, pair2)).fetchone()

    if not existing:
        c.execute("INSERT INTO mutual VALUES (?)", (pair1,))
        db.commit()


def delete_mutual_user(username_one: str, username_two: str):
    c.execute("DELETE FROM mutual WHERE pair = ? OR pair = ?", (username_one + "," + username_two, username_two + "," + username_one))
    db.commit()

def get_mutuals():
    data = c.execute("SELECT * FROM mutual").fetchall()
    return [pair[0] for pair in data]

def find_profile(username: str):
    user = c.execute("SELECT username FROM profiles WHERE username = (?)", (username,)).fetchone()
    return user != None

def get_profile(username: str):
    user = c.execute("SELECT * FROM profiles WHERE username = (?)", (username,)).fetchone()
    if user:
        return {
            "username": user[0],
            "name": user[1],
            "surname": user[2],
            "categories": safe_json_loads(user[3]),
            "tags": safe_json_loads(user[4]),
            "skills": safe_json_loads(user[5]),  # Добавляем навыки
            "events": safe_json_loads(user[6]),
            "description": user[7],
            "swiped_users": safe_json_loads(user[8]),
            "swiped_events": safe_json_loads(user[9]),
            "mail": user[10],
            "vocation": user[11]
        }
    return None

def add_profile_skills(username: str, category: str, skills: str):
    """Добавляет навыки для определенной категории"""
    if not find_profile(username):
        return False
    
    profile = get_profile(username)
    skills_dict = profile.get("skills", {})
    skills_dict[category] = skills 
    
    c.execute("UPDATE profiles SET skills = ? WHERE username = ?", 
              (json.dumps(skills_dict), username))
    db.commit()
    return True

def get_profile_skills(username: str):
    """Возвращает все навыки пользователя по категориям"""
    if not find_profile(username):
        return {}
    
    profile = get_profile(username)
    return profile.get("skills", {})

def safe_json_loads(json_str):
    """Безопасно преобразует строку в JSON, возвращает [] или {} при ошибке"""
    if not json_str or json_str.strip() == '':
        return []
    try:
        return json.loads(json_str)
    except json.JSONDecodeError:
        return []

def add_profile(username: str, name="", surname="", categories="[]", tags="{}", 
                skills="{}", events="[]", description="", swiped_users="[]", 
                swiped_events="[]", mail="", vocation=""):
    if not find_profile(username):
        c.execute("INSERT INTO profiles VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", 
                  (username, name, surname, categories, tags, skills, events, 
                   description, swiped_users, swiped_events, mail, vocation))
        db.commit()

def edit_profile_name(username: str, name: str):
    c.execute("UPDATE profiles SET name = ? WHERE username = ?", (name, username))
    db.commit()

def edit_profile_surname(username: str, surname: str):
    c.execute("UPDATE profiles SET surname = ? WHERE username = ?", (surname, username))
    db.commit()

def add_profile_category(username: str, category: str, tags: str = "", skills: str = ""):
    """Добавляет категорию с тегами и навыками"""
    if not find_profile(username):
        return False
    
    # Обновляем список категорий
    profile = get_profile(username)
    categories = profile.get("categories", [])
    if category not in categories:
        categories.append(category)
    
    # Обновляем теги
    tags_dict = profile.get("tags", {})
    tags_dict[category] = tags  # Сохраняем теги как строку
    
    # Обновляем навыки
    skills_dict = profile.get("skills", {})
    skills_dict[category] = skills  # Сохраняем навыки как строку
    
    # Сохраняем в БД
    c.execute("""
        UPDATE profiles 
        SET categories = ?, tags = ?, skills = ?
        WHERE username = ?
    """, (json.dumps(categories), json.dumps(tags_dict), json.dumps(skills_dict), username))
    db.commit()
    return True

def get_profile_categories(username: str):
    """Возвращает категории с тегами и навыками"""
    if not find_profile(username):
        return []
    
    profile = get_profile(username)
    result = []
    
    for category in profile.get("categories", []):
        result.append({
            "name": category,
            "tags": profile["tags"].get(category, "").split(),  # Теги как список слов
            "skills": profile["skills"].get(category, "").split(", ")  # Навыки как список
        })
    
    return result


def remove_profile_category(username: str, category: str):
    """Удаляет категорию из профиля пользователя вместе с тегами и навыками"""
    if not find_profile(username):
        return False
    
    # Получаем текущий профиль
    profile = get_profile(username)
    
    # Удаляем категорию из списка
    categories = profile.get("categories", [])
    if category in categories:
        categories.remove(category)
    
    # Удаляем теги для этой категории
    tags_dict = profile.get("tags", {})
    if category in tags_dict:
        del tags_dict[category]
    
    # Удаляем навыки для этой категории
    skills_dict = profile.get("skills", {})
    if category in skills_dict:
        del skills_dict[category]
    
    # Обновляем запись в базе данных
    c.execute("""
        UPDATE profiles 
        SET categories = ?, tags = ?, skills = ?
        WHERE username = ?
    """, (
        json.dumps(categories),
        json.dumps(tags_dict),
        json.dumps(skills_dict),
        username
    ))
    db.commit()
    return True


def get_all_profiles():
    """Возвращает список профилей (словарей с данными каждого профиля)"""
    c.execute("SELECT * FROM profiles")
    rows = c.fetchall()
    
    column_names = [description[0] for description in c.description]
    
    profiles_list = []
    for row in rows:
        profile_dict = {column_names[i]: row[i] for i in range(len(column_names))}
        
        # Преобразуем JSON-строки в Python-объекты
        for field in ['categories', 'events', 'swiped_users', 'swiped_events', 'tags', 'skills']:
            if profile_dict.get(field):
                try:
                    profile_dict[field] = json.loads(profile_dict[field])
                except (json.JSONDecodeError, TypeError):
                    profile_dict[field] = [] if field in ['categories', 'events', 'swiped_users', 'swiped_events'] else {}
        
        # Добавляем профиль в список
        profiles_list.append(profile_dict)
    
    return profiles_list

def get_profiles_by_categories(categories: str):
    categories_list = [cat.strip() for cat in categories.split(",")]
    
    c.execute("SELECT * FROM profiles")
    rows = c.fetchall()
    
    column_names = [description[0] for description in c.description]
    
    users_list = []
    
    for row in rows:
        user_dict = {column_names[i]: row[i] for i in range(len(column_names))}
        
        tags_str = user_dict.get("tags", None)
        tags_dict = json.loads(tags_str) if tags_str else {}
        
        filtered_tags = []
        for category in categories_list:
            if category in tags_dict:
                filtered_tags.extend(tags_dict[category])
        
        user_dict["tags"] = filtered_tags
        
        if filtered_tags:
            users_list.append(user_dict)
    
    return users_list


def add_profile_event(username: str, hash: string):
    if not find_profile(username):
        return
    
    events_str = c.execute("SELECT events FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    events_list = json.loads(events_str) if events_str else []
    
    if hash not in events_list:
        events_list.append(hash)
    
    c.execute("UPDATE profiles SET events = ? WHERE username = ?", (json.dumps(events_list), username))
    db.commit()


def delete_profile_event(username: str, hash: string):
    if not find_profile(username):
        return
    
    events_str = c.execute("SELECT events FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    events_list = json.loads(events_str) if events_str else []
    
    if id in events_list:
        events_list.remove(hash)
    
    c.execute("UPDATE profiles SET events = ? WHERE username = ?", (json.dumps(events_list), username))
    db.commit()

def edit_profile_description(username: str, description: str):
    c.execute("UPDATE profiles SET description = ? WHERE username = ?", (description, username))
    db.commit()

def edit_profile_vocation(username: str, vocation: str):
    c.execute("UPDATE profiles SET vocation = ? WHERE username = ?", (vocation, username))
    db.commit()


def add_profile_swiped_user(username: str, user_name_to: str):
    if not find_profile(username):
        return
    
    users_str = c.execute("SELECT swiped_users FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    users_list = json.loads(users_str) if users_str else []

    if user_name_to not in users_list:
        users_list.append(user_name_to)
    
    c.execute("UPDATE profiles SET swiped_users = ? WHERE username = ?", (json.dumps(users_list), username))
    db.commit()
    
    create_notification(user_name_to, username)


def add_profile_swiped_event(username: str, hash: str):
    """Добавляет событие в список свайпнутых событий пользователя"""
    if not find_profile(username):
        return False
    
    swiped_events_str = c.execute("SELECT swiped_events FROM profiles WHERE username = ?", 
                                 (username,)).fetchone()[0]
    swiped_events = json.loads(swiped_events_str) if swiped_events_str else []
    
    if hash not in swiped_events:
        swiped_events.append(hash)
    
    c.execute("UPDATE profiles SET swiped_events = ? WHERE username = ?", 
              (json.dumps(swiped_events), username))
    db.commit()
    return True


def get_profile_swiped_users(username: str):
    """Возвращает список свайпнутых пользователей"""
    if not find_profile(username):
        return []
    
    swiped_users_str = c.execute("SELECT swiped_users FROM profiles WHERE username = ?", 
                                (username,)).fetchone()[0]
    return json.loads(swiped_users_str) if swiped_users_str else []


def get_profile_swiped_events(username: str):
    """Возвращает список свайпнутых событий пользователя"""
    if not find_profile(username):
        return []
    
    swiped_events_str = c.execute("SELECT swiped_events FROM profiles WHERE username = ?", 
                                 (username,)).fetchone()[0]
    return json.loads(swiped_events_str) if swiped_events_str else []


def delete_profile_swiped_user(username: str, username_who: int):
    if not find_profile(username):
        return
    
    users_str = c.execute("SELECT swiped_users FROM profiles WHERE username = ?", (username,)).fetchone()[0]
    users_list = json.loads(users_str) if users_str else []
    
    if username_who in users_list:
        users_list.remove(username_who)
    
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


def add_event(name, owner, categories, description, location, date_from, date_to, public, online):
    hash = generate_hash()
    while (find_event(hash)):
        hash = generate_hash()

    c.execute("INSERT INTO events VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", 
                (hash, owner, "[]", name, categories, description, location, date_from, date_to, public, online))
    db.commit()

    return hash


def add_event_member(hash: str, member_id: int):
    if not find_event(hash):
        return

    members_str = c.execute("SELECT members FROM events WHERE hash = ?", (hash,)).fetchone()[0]
    members_list = json.loads(members_str) if members_str else []
    
    if member_id not in members_list:
        members_list.append(member_id)
    
    c.execute("UPDATE events SET members = ? WHERE hash = ?", (json.dumps(members_list), hash))
    db.commit()


def delete_event_member(hash: str, member: int):
    if not find_event(hash):
        return
    
    member_str = c.execute("SELECT member FROM events WHERE hash = ?", (hash,)).fetchone()[0]
    member_list = json.loads(member_str) if member_str else []
    
    if member in member_list:
        member_list.remove(member)
    
    c.execute("UPDATE events SET member = ? WHERE hash = ?", (json.dumps(member_list), hash))
    db.commit()


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


def edit_event_date(hash: str, date_from: str, date_to: str):
    c.execute("UPDATE events SET date_from = ? WHERE hash = ?", (date_from, hash))
    c.execute("UPDATE events SET date_to = ? WHERE hash = ?", (date_to, hash))
    db.commit()


def delete_event(hash: str):
    c.execute("DELETE FROM events WHERE hash = ?", (hash,))
    db.commit()


def get_events_by_categories(categories: str):
    categories_list = [cat.strip() for cat in categories.split(",")]
    
    conditions = " OR ".join([f"categories LIKE ?" for _ in categories_list])
    params = [f"%{cat}%" for cat in categories_list]
    
    query = f"SELECT * FROM events WHERE {conditions}"
    c.execute(query, params)
    rows = c.fetchall()
    
    column_names = [description[0] for description in c.description]
    
    events_list = []
    for row in rows:
        event_dict = {column_names[i]: row[i] for i in range(len(column_names))}
        
        if event_dict["categories"]:
            event_dict["categories"] = [cat.strip() for cat in event_dict["categories"].split(",")]
        else:
            event_dict["categories"] = []
        
        if event_dict["members"]:
            try:
                event_dict["members"] = json.loads(event_dict["members"])  
            except json.JSONDecodeError:
                event_dict["members"] = []
        else:
            event_dict["members"] = []
        
        events_list.append(event_dict)
    
    return events_list


def add_event_member(event_hash: str, member: str):
    c.execute("SELECT members FROM events WHERE hash = ?", (event_hash,))
    result = c.fetchone()
    
    if not result:
        return
    
    members_str = result[0]
    members_list = json.loads(members_str) if members_str else []
    
    if member not in members_list:
        members_list.append(member)
        
        c.execute("UPDATE events SET members = ? WHERE hash = ?", (json.dumps(members_list), event_hash))
        db.commit()


def get_all_events():
    c.execute("SELECT * FROM events")
    rows = c.fetchall()
    
    column_names = [description[0] for description in c.description]
    
    events_list = []
    for row in rows:
        event_dict = {column_names[i]: row[i] for i in range(len(column_names))}
        
        if event_dict["categories"]:
            event_dict["categories"] = [cat.strip() for cat in event_dict["categories"].split(",")]
        else:
            event_dict["categories"] = []
        
        if event_dict["members"]:
            try:
                event_dict["members"] = json.loads(event_dict["members"])
            except json.JSONDecodeError:
                event_dict["members"] = [] 
        else:
            event_dict["members"] = []
        
        events_list.append(event_dict)
    
    return events_list


def is_user_and_password_correct(username: str, password: str):
    user = c.execute("SELECT * FROM users_passwords WHERE username = ? AND password = ?", (username, password)).fetchone()
    return user != None


def add_username_and_password(username: str, password: str):
    if not is_exist_username(username):
        c.execute("INSERT INTO users_passwords VALUES (?, ?)", (username, password))
        db.commit()
        add_profile(username)


def is_exist_username(username: str):
    username = c.execute("SELECT * FROM users_passwords WHERE username = ?", (username,)).fetchone()
    return username != None


def get_all_user_events(username: str):
    """
    Получает все события пользователя по его username.
    Возвращает список словарей с данными о событиях.
    """
    if not find_profile(username):
        return []
    
    all_events = get_all_events()
    user_events = []

    for event in all_events:
        if event["owner"] == username or username in event["members"]:
            user_events.append(event)

    return user_events


def add_swiped_event(username: str, event_hash: str):
    """Добавляет событие в список свайпнутых событий пользователя"""
    if not find_profile(username):
        return False
    
    swiped_events_str = c.execute("SELECT swiped_events FROM profiles WHERE username = ?", 
                                 (username,)).fetchone()[0]
    swiped_events = json.loads(swiped_events_str) if swiped_events_str else []
    
    if event_hash not in swiped_events:
        swiped_events.append(event_hash)
    
    c.execute("UPDATE profiles SET swiped_events = ? WHERE username = ?", 
              (json.dumps(swiped_events), username))
    db.commit()
    return True


def remove_swiped_event(username: str, event_hash: str):
    """Удаляет событие из списка свайпнутых событий пользователя"""
    if not find_profile(username):
        return False
    
    swiped_events_str = c.execute("SELECT swiped_events FROM profiles WHERE username = ?", 
                                 (username,)).fetchone()[0]
    swiped_events = json.loads(swiped_events_str) if swiped_events_str else []
    
    if event_hash in swiped_events:
        swiped_events.remove(event_hash)
    
    c.execute("UPDATE profiles SET swiped_events = ? WHERE username = ?", 
              (json.dumps(swiped_events), username))
    db.commit()
    return True


def get_swiped_events(username: str):
    """Возвращает список свайпнутых событий пользователя"""
    if not find_profile(username):
        return []
    
    swiped_events_str = c.execute("SELECT swiped_events FROM profiles WHERE username = ?", 
                                 (username,)).fetchone()[0]
    return json.loads(swiped_events_str) if swiped_events_str else []

