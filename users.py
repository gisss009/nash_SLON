from flask import Blueprint
from db import *
from flask import request, jsonify

bp1 = Blueprint('users', __name__,)

@bp1.route("/users/get_all_profiles")
def get_all_profiles_handler():
    return jsonify({"ok": True, "response": get_all_profiles()}), 200


@bp1.route("/users/get_all_user_events")
def get_all_user_events_handler():
    username = request.args.get('username')
    if not username:
        return jsonify({"ok": False, "response": "parameter username is missing."}), 400

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."}), 404

    return jsonify({"ok": True, "response": get_all_user_events(username)}), 200


@bp1.route("/add_username_and_password")
def add_username_and_password_handler():
    username = request.args.get('username')
    password = request.args.get('password')

    if not username or not password:
        return jsonify({"ok": False, "response": "parameters username or password is missing."})

    if is_exist_username(username):
        return jsonify({"ok": False, "response": "this username is already exists."})

    add_username_and_password(username, password)
    return jsonify({"ok": True}), 200


@bp1.route("/is_username_and_password_correct", methods=['POST', 'GET'])
def is_username_and_password_handler():
    username = request.args.get('username')
    password = request.args.get('password')

    if not username or not password:
        return jsonify({"ok": False, "response": "parameters username or password is missing."})

    return jsonify({"ok": True, "response": is_user_and_password_correct(username, password)}), 200


@bp1.route("/users/find_profile")
def find_profile_handler():
    username = request.args.get('username')

    if not username:
        return jsonify({"ok": False, "response": "parameter username is missing."})

    return jsonify({"ok": True, "response": find_profile(username)}), 200


@bp1.route("/users/get_profile")
def get_profile_handler():
    username = request.args.get('username')

    if not username:
        return jsonify({"ok": False, "response": "parameter username is missing."})

    profile = get_profile(username)
    if not profile:
        return jsonify({"ok": False, "response": "username with current username not found."}), 404

    return jsonify({"ok": True, "response": profile}), 200


@bp1.route("/users/add_profile")
def add_profile_handler():
    username = request.args.get('username')

    if not username:
        return jsonify({"ok": False, "response": "parameter username is missing."})

    if find_profile(username):
        return jsonify({"ok": False, "response": "username is alreay exist."})

    add_profile(username)

    return jsonify({"ok": True}), 200


@bp1.route("/users/edit_profile_name")
def edit_profile_name_handler():
    username = request.args.get('username')
    name = request.args.get('name')

    if not username or not name:
        return jsonify({"ok": False, "response": "parameter username or name is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    edit_profile_name(username, name)

    return jsonify({"ok": True}), 200


@bp1.route("/users/edit_profile_surname")
def edit_profile_surname_handler():
    username = request.args.get('username')
    surname = request.args.get('surname')

    if not username or not surname:
        return jsonify({"ok": False, "response": "parameter username or surname is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "current username not found."})

    edit_profile_name(username, surname)

    return jsonify({"ok": True}), 200

@bp1.route("/users/add_category_with_tags", methods=['GET'])
def add_category_with_tags_handler():
    username = request.args.get('username')
    category = request.args.get('category')
    tags = request.args.get('tags', "")  # "Python SQL DevOps"
    skills = request.args.get('skills', "")  # "Backend, Cloud"
    
    if not username or not category:
        return jsonify({"ok": False, "response": "Missing username or category"}), 400
    
    success = add_profile_category(username, category, tags, skills)
    return jsonify({"ok": success}), 200 if success else 400

@bp1.route("/users/get_categories", methods=['GET'])
def get_categories_handler():
    username = request.args.get('username')
    if not username:
        return jsonify({"ok": False, "response": "Missing username"}), 400
    
    categories = get_profile_categories(username)
    return jsonify({"ok": True, "response": categories}), 200


@bp1.route("/users/delete_profile_category")
def delete_profile_category_handler():
    username = request.args.get('username')
    category = request.args.get('category')

    if not username or not category:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    if category not in ["IT", "Social", "Business", "Health", "Creation", "Sport", "Education", "Science"]:
        return jsonify({"ok": False, "response": "category not in accessed categories"})

    remove_profile_category(username, category)

    return jsonify({"ok": True}), 200


@bp1.route("/users/add_profile_event")
def add_profile_event_handler():
    username = request.args.get('username')
    hash = request.args.get('hash')

    if not username or not hash:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    add_profile_event(username, hash)

    return jsonify({"ok": True}), 200


@bp1.route("/users/delete_profile_event")
def delete_profile_event_handler():
    username = request.args.get('username')
    id = request.args.get('id')

    if not username or not id:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    delete_profile_event(username, id)

    return jsonify({"ok": True}), 200

@bp1.route("/users/edit_profile_description")
def edit_profile_description_handler():
    username = request.args.get('username')
    description = request.args.get('description')

    if not username or not description:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    edit_profile_description(username, description)

    return jsonify({"ok": True}), 200


@bp1.route("/users/edit_profile_vocation")
def edit_profile_vocation_handler():
    username = request.args.get('username')
    vocation = request.args.get('vocation')

    if not username or not vocation:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    edit_profile_vocation(username, vocation)

    return jsonify({"ok": True}), 200


@bp1.route("/users/add_profile_swiped_user")
def add_profile_swiped_user_handler():
    username = request.args.get('username')
    username_to = request.args.get('username_to')

    if not username or not username_to:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    add_profile_swiped_user(username, username_to)

    return jsonify({"ok": True}), 200


@bp1.route("/users/delete_profile_swiped_user")
def delete_profile_swiped_user_handler():
    username = request.args.get('username')
    username_to = request.args.get('username_to')

    if not username or not username_to:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username) or not find_profile(username_to):
        return jsonify({"ok": False, "response": "username with current username not found."})
    
    delete_profile_swiped_user(username, username_to)

    return jsonify({"ok": True}), 200


@bp1.route('/users/get_swiped_users', methods=['GET'])
def get_swiped_users_handler():
    username = request.args.get('username')
    if not username:
        return jsonify({'ok': False, 'error': 'Username is required'}), 400
    
    swiped_users = get_profile_swiped_users(username)
    user_profiles = []
    
    for user in swiped_users:
        profile = get_profile(user) 
        if profile:
            user_profiles.append({
                'username': profile['username'],
                'name': profile['name'],
                'surname': profile["surname"],
                'tags': profile.get('tags', []),
                'vocation': profile.get('vocation', ''),
                'info': profile.get('description', ''),
                'skills': profile.get('skills', ''),
                'is_mutual': check_if_mutual(username, user),
                'is_accepted_me': check_if_accepted(username, user),
                'is_i_liked_him': True
            })
    
    return jsonify({'ok': True, 'response': user_profiles})

def check_if_mutual(user1, user2):
    """Check if both users swiped each other"""
    user1_swiped = get_profile_swiped_users(user1)
    user2_swiped = get_profile_swiped_users(user2)
    return user2 in user1_swiped and user1 in user2_swiped

def check_if_accepted(me, other_user):
    """Check if the other user has swiped me"""
    other_swiped = get_profile_swiped_users(other_user)
    return me in other_swiped



@bp1.route("/users/edit_profile_mail")
def edit_profile_mail_handler():
    username = request.args.get('username')
    mail = request.args.get('mail')

    if not username or not id:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    if is_email_exist(mail):
        return jsonify({"ok": False, "response": "email is already exist."})

    edit_profile_mail(username, mail)

    return jsonify({"ok": True}), 200


@bp1.route("/users/update_profile", methods=['GET'])
def update_profile_handler():
    try:
        username = request.args.get('username')
        if not username:
            return jsonify({"ok": False, "response": "parameter username is missing."}), 400

        # Получаем все возможные параметры для обновления
        updates = {
            'name': request.args.get('name'),
            'surname': request.args.get('surname'),
            'vocation': request.args.get('vocation'),
            'description': request.args.get('description'),
            'mail': request.args.get('mail'),
        }

        # Фильтруем None значения
        updates = {k: v for k, v in updates.items() if v is not None}

        if not updates:
            return jsonify({"ok": False, "response": "no parameters to update."}), 400

        # Проверяем существование пользователя
        if not find_profile(username):
            return jsonify({"ok": False, "response": "username not found."}), 404

        # Обновляем каждое поле
        if 'name' in updates:
            edit_profile_name(username, updates['name'])
        if 'surname' in updates:
            edit_profile_surname(username, updates['surname'])
        if 'vocation' in updates:
            edit_profile_vocation(username, updates['vocation'])
        if 'description' in updates:
            edit_profile_description(username, updates['description'])
        if 'mail' in updates:
            if is_email_exist(updates['mail']):
                return jsonify({"ok": False, "response": "email already exists."}), 400
            edit_profile_mail(username, updates['mail'])

        db.commit()
        return jsonify({"ok": True}), 200

    except Exception as e:
        print(f"Error updating profile: {str(e)}")
        db.rollback()
        return jsonify({"ok": False, "response": "internal server error"}), 500

