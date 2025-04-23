from flask import Blueprint, request, jsonify, current_app
from db import *
import json

bp2 = Blueprint('events', __name__)

@bp2.route("/events/get_event")
def get_event_handler():
    hash = request.args.get('hash')
    if not hash:
        return jsonify({"ok": False, "response": "parameter hash is missing."}), 400
    if not find_event(hash):
        return jsonify({"ok": False, "response": "hash with current hash not found."}), 404

    event = get_event(hash)
    ans = {
        "hash":        event[0],
        "name":        event[3],
        "categories":  json.loads(event[4]) if event[4] else [],
        "description": event[5],
        "location":    event[6],
        "date_from":   event[7],
        "date_to":     event[8],
        "public":      event[9],
        "online":      event[10]
    }
    return jsonify({"ok": True, "response": ans}), 200


@bp2.route("/events", methods=['POST'])
def create_event_handler():
    """
    Создание нового события.
    Ожидает JSON body:
    {
      "name":        str,
      "owner":       str,
      "categories":  [str,...],
      "description": str,
      "location":    str,
      "date_from":   "dd.MM.yyyy",
      "date_to":     "dd.MM.yyyy",
      "public":      0|1,
      "online":      0|1
    }
    Возвращает полный объект события в JSON.
    """
    data = request.get_json(force=True)
    # 1) Проверяем наличие всех полей
    required = ["name","owner","categories","description","location","date_from","date_to","public","online"]
    for f in required:
        if f not in data:
            return jsonify(ok=False, response=f"Missing field: {f}"), 400

    name        = data["name"]
    owner       = data["owner"]
    cats        = data["categories"]
    description = data["description"]
    location    = data["location"]
    date_from   = data["date_from"]
    date_to     = data["date_to"]
    public      = int(data["public"])
    online      = int(data["online"])

    if not isinstance(cats, list) or any(not isinstance(c, str) for c in cats):
        return jsonify(ok=False, response="categories must be a list of strings"), 400

    try:
        db.execute("BEGIN")  # старт транзакции

        # 2) Добавляем событие, сохраняя категории как JSON-массив
        new_hash = add_event(
            name,
            owner,
            json.dumps(cats),
            description,
            location,
            date_from,
            date_to,
            public,
            online
        )

        # 3) Добавляем связи в таблицу event_categories
        for cat in cats:
            if cat in ["IT","Social","Business","Health","Creation","Sport","Education","Science"]:
                add_event_category(new_hash, cat)

        # 4) Извлекаем только что созданную запись
        row = c.execute("SELECT * FROM events WHERE hash = ?", (new_hash,)).fetchone()
        db.commit()  # COMMIT транзакции

    except Exception as e:
        current_app.logger.error(f"Create event failed: {e}")
        db.rollback()
        return jsonify(ok=False, response="internal server error"), 500

    # 5) Формируем и возвращаем полный объект события
    event_obj = {
        "hash":        row[0],
        "owner":       row[1],
        "members":     json.loads(row[2] or "[]"),
        "name":        row[3],
        "categories":  json.loads(row[4] or "[]"),
        "description": row[5],
        "location":    row[6],
        "date_from":   row[7],
        "date_to":     row[8],
        "public":      row[9],
        "online":      row[10]
    }

    resp = jsonify(ok=True, response=event_obj)
    resp.status_code = 201
    resp.headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0"
    return resp

@bp2.route("/events/edit_event_name")
def edit_event_name_handler():
    return jsonify({"ok": False, "response": "Changing event name is forbidden."}), 403

@bp2.route("/events/edit_event_category")
def add_event_category_handler():
    hash = request.args.get('hash')
    category = request.args.get('category')
    if not hash or not category:
        return jsonify({"ok": False, "response": "parameter hash or category is missing."}), 400
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event not found."}), 404
    add_event_category(hash, category)
    return jsonify({"ok": True}), 200

@bp2.route("/events/delete_event_category")
def delete_event_category_handler():
    hash = request.args.get('hash')
    category = request.args.get('category')
    if not hash or not category:
        return jsonify({"ok": False, "response": "parameter hash or category is missing."}), 400
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event not found."}), 404
    delete_event_category(hash, category)
    return jsonify({"ok": True}), 200

@bp2.route("/events/edit_event_description")
def edit_event_description_handler():
    hash = request.args.get('hash')
    description = request.args.get('description')
    if not hash or not description:
        return jsonify({"ok": False, "response": "parameter hash or description is missing."}), 400
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event not found."}), 404
    edit_event_description(hash, description)
    return jsonify({"ok": True}), 200

@bp2.route("/events/edit_event_location")
def edit_event_location_handler():
    hash = request.args.get('hash')
    location = request.args.get('location')
    if not hash or not location:
        return jsonify({"ok": False, "response": "parameter hash or location is missing."}), 400
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event not found."}), 404
    edit_event_location(hash, location)
    return jsonify({"ok": True}), 200

@bp2.route("/events/edit_event_date")
def edit_event_date_handler():
    hash = request.args.get('hash')
    date_from = request.args.get('date_from')
    date_to = request.args.get('date_to')
    if not hash or not date_from or not date_to:
        return jsonify({"ok": False, "response": "one of parameters is missing."}), 400
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event not found."}), 404
    edit_event_date(hash, date_from, date_to)
    return jsonify({"ok": True}), 200

@bp2.route("/events/add_event_member")
def add_event_member_handler():
    username = request.args.get('username')
    hash = request.args.get('hash')
    if not hash or not username:
        return jsonify({"ok": False, "response": "parameter hash or username is missing."}), 400
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event not found."}), 404
    if not find_profile(username):
        return jsonify({"ok": False, "response": "username not found."}), 404
    add_event_member(hash, username)
    return jsonify({"ok": True}), 200

@bp2.route("/events/delete_event")
def delete_event_handler():
    hash = request.args.get('hash')
    if not hash:
        return jsonify({"ok": False, "response": "parameter hash is missing."}), 400
    delete_event(hash)
    return jsonify({"ok": True}), 200

@bp2.route("/events/get_all_events")
def get_all_events_handler():
    events = get_all_events()
    for e in events:
        if isinstance(e.get("categories"), str):
            try:
                e["categories"] = json.loads(e["categories"])
            except json.JSONDecodeError:
                e["categories"] = []
        else:
            e["categories"] = e.get("categories", [])
        if isinstance(e.get("members"), str):
            try:
                e["members"] = json.loads(e["members"])
            except json.JSONDecodeError:
                e["members"] = []
        else:
            e["members"] = e.get("members", [])

    return jsonify({"ok": True, "response": events}), 200


@bp2.route("/events/<event_hash>", methods=["PUT"])
def update_event_handler(event_hash):
    """
    Обновляет существующее событие.
    Проверяет:
      - наличие ивента (404)
      - заполненность полей (400)
      - отсутствие другого ивента с тем же name (409)
    """
    data = request.get_json(force=True) or {}
    required = ["name","categories","description","location","date_from","date_to","public","online"]
    # 1) Проверяем все поля
    for f in required:
        if f not in data:
            return jsonify(ok=False, response=f"Missing field: {f}"), 400

    if not find_event(event_hash):
        return jsonify(ok=False, response="Event not found."), 404

    name        = data["name"].strip()
    cats        = data["categories"]
    description = data["description"].strip()
    location    = data["location"].strip()
    date_from   = data["date_from"].strip()
    date_to     = data["date_to"].strip()
    try:
        public = int(data["public"])
        online = int(data["online"])
    except:
        return jsonify(ok=False, response="public and online must be 0 or 1"), 400

    # 2) Проверяем непустоту
    if not all([name, description, location, date_from, date_to]) \
       or not isinstance(cats, list) or len(cats) == 0:
        return jsonify(ok=False, response="All fields must be filled"), 400

    # 3) Запрещаем переименование в имя другого ивента
    dup = c.execute(
        "SELECT 1 FROM events WHERE name = ? AND hash != ?",
        (name, event_hash)
    ).fetchone()
    if dup:
        return jsonify(ok=False, response="Another event with this name already exists"), 409

    try:
        db.execute("BEGIN")

        # 4) Обновляем саму запись, в том числе JSON-поле categories
        c.execute("""
            UPDATE events SET
                name        = ?,
                categories  = ?,
                description = ?,
                location    = ?,
                date_from   = ?,
                date_to     = ?,
                public      = ?,
                online      = ?
            WHERE hash = ?
        """, (
            name,
            json.dumps(cats),
            description,
            location,
            date_from,
            date_to,
            public,
            online,
            event_hash
        ))

        # 5) Читаем обновлённую запись
        row = c.execute("SELECT * FROM events WHERE hash = ?", (event_hash,)).fetchone()
        db.commit()

    except Exception as e:
        current_app.logger.error(f"Update event failed: {e}")
        db.rollback()
        return jsonify(ok=False, response="Internal server error"), 500

    event_obj = {
        "hash":        row[0],
        "owner":       row[1],
        "members":     json.loads(row[2] or "[]"),
        "name":        row[3],
        "categories":  json.loads(row[4] or "[]"),
        "description": row[5],
        "location":    row[6],
        "date_from":   row[7],
        "date_to":     row[8],
        "public":      row[9],
        "online":      row[10]
    }
    return jsonify(ok=True, response=event_obj), 200

