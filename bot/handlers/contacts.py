from aiogram import Router, types, F
from database.db import Database
from keyboards.inline import get_contacts_keyboard, get_main_menu_keyboard
router = Router()

@router.callback_query(F.data == "main_contacts")
async def show_contacts(callback: types.CallbackQuery, db: Database):
    contacts = await db.get_all_contacts()
    if not contacts:
        await callback.message.edit_text(
            "📞 **Контакты**\n\n"
            "Информация о контактах временно недоступна.\n"
            "Пожалуйста, зайдите позже.",
            parse_mode="Markdown",
            reply_markup=get_contacts_keyboard()
        )
        await callback.answer()
        return
    text = "📞 **Наши контакты**\n\n"
    for contact in contacts:
        city_name = contact.get('city_name', 'Город')
        text += f"🏙️ **{city_name}**\n"
        if contact.get('addresses'):
            addresses = contact['addresses']
            if isinstance(addresses, list):
                for addr in addresses:
                    address = addr.get('address', '')
                    schedule = addr.get('schedule', '')
                    text += f"📍 {address}"
                    if schedule:
                        text += f" ({schedule})"
                    text += "\n"
            else:
                text += f"📍 {addresses}\n"
        if contact.get('phones'):
            phones = contact['phones']
            if isinstance(phones, list):
                text += f"📱 {', '.join(phones)}\n"
            else:
                text += f"📱 {phones}\n"
        if contact.get('email'):
            text += f"📧 {contact['email']}\n"
        if contact.get('social_links'):
            social = contact['social_links']
            if isinstance(social, dict):
                for platform, link in social.items():
                    if platform == 'vk':
                        text += f"💬 VK: {link}\n"
                    elif platform == 'telegram':
                        text += f"💬 Telegram: {link}\n"
                    else:
                        text += f"💬 {platform.capitalize()}: {link}\n"
        text += "\n"
    
    await callback.message.edit_text(
        text,
        parse_mode="Markdown",
        reply_markup=get_contacts_keyboard()
    )
    await callback.answer()

@router.callback_query(F.data == "main_menu")
async def back_to_main_menu(callback: types.CallbackQuery):
    await callback.message.edit_text(
        "🏠 **Главное меню**\n\n"
        "Выберите действие:",
        parse_mode="Markdown",
        reply_markup=get_main_menu_keyboard()
    )
    await callback.answer()
