from aiogram.types import InlineKeyboardMarkup, InlineKeyboardButton
from aiogram.utils.keyboard import InlineKeyboardBuilder

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
            callback_data=f"{prefix}_{cat['id']}")
    builder.adjust(2)
    return builder.as_markup()

def get_product_keyboard():
    keyboard = InlineKeyboardMarkup(inline_keyboard=[
        [
            InlineKeyboardButton(
                text="🔙 Назад в каталог",
                callback_data="back_to_catalog"
            )
        ]
    ])
    return keyboard


###
def get_admin_keyboard():
    keyboard = InlineKeyboardMarkup(inline_keyboard=[
        [
            InlineKeyboardButton(
                text="📦 Управление товарами",
                callback_data="admin_products"
            )
        ],
        [
            InlineKeyboardButton(
                text="📊 Статистика",
                callback_data="admin_stats"
            )
        ],
        [
            InlineKeyboardButton(
                text="👥 Пользователи",
                callback_data="admin_users"
            )
        ]
    ])
    return keyboard
