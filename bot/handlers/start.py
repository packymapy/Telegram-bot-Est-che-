from aiogram import Router, types, F
from aiogram.filters import Command
from aiogram.fsm.context import FSMContext
from aiogram.fsm.state import State, StatesGroup
from datetime import datetime
from database.db import Database
from keyboards.inline import (get_terms_keyboard, get_retry_keyboard, get_main_menu_keyboard, get_city_keyboard)
router = Router()

class AgeVerification(StatesGroup):
    waiting_birth_date = State()
    waiting_city = State()

@router.message(Command("start"))
async def cmd_start(message: types.Message, state: FSMContext, db: Database):
    user_id = message.from_user.id
    await db.create_user_if_not_exists(
        tg_id=user_id,
        first_name=message.from_user.first_name,
        last_name=message.from_user.last_name,
        username=message.from_user.username)
    can_access, reason, age_verified, agreed_to_terms, is_blocked, attempts_left, block_until, age = await db.check_user_access(user_id)
    if can_access:
        user_city = await db.get_user_city(user_id)
        city_text = f" (г. {user_city})" if user_city else ""
        await message.answer(
            f"✅ Добро пожаловать, {message.from_user.first_name}!{city_text}",
            reply_markup=get_main_menu_keyboard())
        return
    if is_blocked and block_until:
        minutes_left = int((block_until - datetime.now()).total_seconds() / 60) + 1
        await message.answer(
            f"🔒 Аккаунт заблокирован до {block_until.strftime('%H:%M')}\n"
            f"⏳ Осталось минут: {minutes_left}\n\n"
            f"Причина: {reason}")
        return
    if agreed_to_terms and not age_verified:
        await message.answer(
            "📋 Вы уже приняли условия использования.\n\n"
            "Теперь укажите вашу дату рождения в формате ДД.ММ.ГГГГ\n"
            "Например: 01.07.2006")
        await state.set_state(AgeVerification.waiting_birth_date)
        return
    if attempts_left <= 0:
        await message.answer(
            "🔒 Вы превысили допустимое количество попыток.\n"
            "Аккаунт заблокирован на 60 минут.\n\n"
            "Попробуйте позже.")
        return
    welcome_text = f"""
👋 Привет, {message.from_user.first_name}!

Добро пожаловать в наш магазин!

Для получения доступа к каталогу необходимо:
1️⃣ Подтвердить ваш возраст (18+)
2️⃣ Принять условия использования

📅 Пожалуйста, введите вашу дату рождения в формате ДД.ММ.ГГГГ:
Пример: 01.07.2006
    """
    await message.answer(welcome_text)
    await state.set_state(AgeVerification.waiting_birth_date)

@router.message(AgeVerification.waiting_birth_date)
async def process_birth_date(message: types.Message, state: FSMContext, db: Database):
    user_id = message.from_user.id
    try:
        birth_date = datetime.strptime(message.text.strip(), "%d.%m.%Y").date()
    except ValueError:
        await message.answer(
            "❌ Неверный формат даты!\n\n"
            "Пожалуйста, введите дату в формате ДД.ММ.ГГГГ\n"
            "Пример: 01.07.2006")
        return
    if birth_date > datetime.now().date():
        await message.answer(
            "❌ Дата рождения не может быть в будущем!\n\n"
            "Пожалуйста, введите корректную дату.")
        return
    if birth_date < datetime.now().replace(year=datetime.now().year - 100).date():
        await message.answer(
            "❌ Похоже, вы слишком стары для этого бота 😄\n"
            "Пожалуйста, введите корректную дату.")
        return
    success, msg, is_adult = await db.set_birth_date(user_id, birth_date)
    if not success:
        await message.answer(f"❌ {msg}")
        await state.clear()
        return
    if not is_adult:
        await message.answer(
            "❌ Доступ запрещен!\n\n"
            "Вам должно быть больше 18 лет для доступа к нашему каталогу.\n\n"
            "Аккаунт заблокирован на 60 минут.\n"
            "Попробуйте позже с другой датой рождения.")
        await state.clear()
        return
    terms_text = """
**Условия использования**

В соответствии с Федеральным законом Российской Федерации от 23 февраля 2013 г. N 15-ФЗ «Об охране здоровья граждан от воздействия окружающего табачного дыма и последствий потребления табака»
Для дальнейшего просмотра сайта каталога, Вам необходимо подтвердить свое совершеннолетие и дать согласие на просмотр фото никотинсодержащей продукции и аксессуаров к ней на основании Закона РФ «О защите прав потребителей» от 07.02.1992 N 2300-1 (действующая редакция от 24.04.2020).

1. Я принимаю [условия пользования ботом](https://vape-shop43.ru/consent), и прошу показать весь товар в каталоге.

2. Я подтверждаю, что мне уже исполнилось 18 лет.

Для продолжения необходимо принять условия.
    """
    await message.answer(
        terms_text,
        parse_mode="Markdown",
        reply_markup=get_terms_keyboard())
    await state.clear()

@router.callback_query(F.data == "terms_accept")
async def accept_terms(callback: types.CallbackQuery, state: FSMContext, db: Database):
    user_id = callback.from_user.id
    success, message = await db.accept_terms(user_id)
    if not success:
        await callback.answer(message, show_alert=True)
        return
    user_city = await db.get_user_city(user_id)
    if user_city:
        await callback.message.edit_text(
            f"✅ **Условия приняты!**\n\n"
            f"🎉 Добро пожаловать в магазин!\n"
            f"📍 Ваш город: {user_city}",
            parse_mode="Markdown")
        await callback.message.answer(
            "🏠 **Главное меню**\n\nВыберите действие:",
            parse_mode="Markdown",
            reply_markup=get_main_menu_keyboard())
    else:
        cities = await db.get_cities()
        if not cities:
            await callback.message.edit_text(
                "✅ **Условия приняты!**\n\n"
                "🎉 Добро пожаловать в магазин!",
                parse_mode="Markdown")
            await callback.message.answer(
                "🏠 **Главное меню**\n\nВыберите действие:",
                parse_mode="Markdown",
                reply_markup=get_main_menu_keyboard())
        else:
            await callback.message.edit_text(
                "✅ **Условия приняты!**\n\n"
                "📍 Последний вопрос - из какого вы города?\n\n"
                "Выберите ваш город из списка:",
                parse_mode="Markdown",
                reply_markup=get_city_keyboard(cities))
            await state.set_state(AgeVerification.waiting_city)
    await callback.answer()
    
@router.callback_query(F.data.startswith("city_"))
async def select_city(callback: types.CallbackQuery, state: FSMContext, db: Database):
    user_id = callback.from_user.id
    city_id = int(callback.data.split("_")[1])
    await db.set_user_city(user_id, city_id)
    cities = await db.get_cities()
    city_name = next((c['name'] for c in cities if c['id'] == city_id), "Город")
    await callback.message.edit_text(
        f"✅ **Отлично!**\n\n"
        f"📍 Ваш город: {city_name}\n\n"
        f"🎉 Добро пожаловать в магазин!",
        parse_mode="Markdown")
    await callback.message.answer(
        "🏠 **Главное меню**\n\nВыберите действие:",
        parse_mode="Markdown",
        reply_markup=get_main_menu_keyboard())
    await state.clear()
    await callback.answer()

@router.callback_query(F.data == "terms_decline")
async def decline_terms(callback: types.CallbackQuery, db: Database):
    user_id = callback.from_user.id
    success, message, attempts_left, is_blocked = await db.decline_terms(user_id)
    if is_blocked:
        await callback.message.edit_text(
            "🔒 **Вы превысили количество попыток!**\n\n"
            "Аккаунт заблокирован на 60 минут.\n"
            "Попробуйте позже.",
            parse_mode="Markdown")
        await callback.answer()
        return
    await callback.message.edit_text(
        f"❌ **Вы отклонили условия использования.**\n\n"
        f"Для доступа к каталогу необходимо принять условия.\n"
        f"Осталось попыток: {attempts_left}\n\n"
        f"Хотите попробовать снова?",
        parse_mode="Markdown",
        reply_markup=get_retry_keyboard())
    await callback.answer()

@router.callback_query(F.data == "retry_verification")
async def retry_verification(callback: types.CallbackQuery, state: FSMContext, db: Database):
    user_id = callback.from_user.id
    await db.reset_verification(user_id)
    await callback.message.edit_text(
        "🔄 **Проверка возраста**\n\n"
        "Пожалуйста, введите вашу дату рождения в формате ДД.ММ.ГГГГ:\n"
        "Пример: 01.07.2006")
    await state.set_state(AgeVerification.waiting_birth_date)
    await callback.answer()
