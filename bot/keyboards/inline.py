from aiogram.types import InlineKeyboardMarkup, InlineKeyboardButton
from aiogram.utils.keyboard import InlineKeyboardBuilder

def get_main_menu_keyboard():
    keyboard = InlineKeyboardMarkup(inline_keyboard=[
        [
            InlineKeyboardButton(
                text="🛍️ Каталог",
                callback_data="main_catalog"
            )
        ],
        [
            InlineKeyboardButton(
                text="📞 Контакты",
                callback_data="main_contacts"
            )
        ]
    ])
    return keyboard

def get_terms_keyboard():
    keyboard = InlineKeyboardMarkup(inline_keyboard=[
        [
            InlineKeyboardButton(
                text="✅ Принимаю условия",
                callback_data="terms_accept"
            )
        ],
        [
            InlineKeyboardButton(
                text="❌ Не принимаю",
                callback_data="terms_decline"
            )
        ]
    ])
    return keyboard

def get_retry_keyboard():
    keyboard = InlineKeyboardMarkup(inline_keyboard=[
        [
            InlineKeyboardButton(
                text="🔄 Пройти проверку заново",
                callback_data="retry_verification"
            )
        ]
    ])
    return keyboard

def get_category_keyboard(categories: list, prefix: str = "cat"):
    builder = InlineKeyboardBuilder()
    for cat in categories:
        builder.button(
            text=cat['name'],
            callback_data=f"{prefix}_{cat['id']}"
        )
    builder.button(
        text="🔙 Главное меню",
        callback_data="main_menu"
    )
    return builder.as_markup()

def get_product_keyboard():
    keyboard = InlineKeyboardMarkup(inline_keyboard=[
        [
            InlineKeyboardButton(
                text="🔙 Назад в каталог",
                callback_data="back_to_catalog"
            )
        ],
        [
            InlineKeyboardButton(
                text="🏠 Главное меню",
                callback_data="main_menu"
            )
        ]
    ])
    return keyboard

def get_contacts_keyboard():
    """Клавиатура для страницы контактов"""
    keyboard = InlineKeyboardMarkup(inline_keyboard=[
        [
            InlineKeyboardButton(
                text="🏠 Главное меню",
                callback_data="main_menu"
            )
        ]
    ])
    return keyboard

def get_catalog_back_keyboard():
    keyboard = InlineKeyboardMarkup(inline_keyboard=[
        [
            InlineKeyboardButton(
                text="🔙 Вернуться в каталог",
                callback_data="back_to_catalog"
            )
        ],
        [
            InlineKeyboardButton(
                text="🏠 Главное меню",
                callback_data="main_menu"
            )
        ]
    ])
    return keyboard
