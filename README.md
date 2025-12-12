Maziya Inn Pub Vleis 🍽️
https://img.shields.io/badge/Type-Restaurant_Management_System-blue
https://img.shields.io/badge/Status-Active-brightgreen
https://img.shields.io/badge/License-MIT-yellow

A comprehensive restaurant management system for Maziya Inn Pub, designed to streamline operations, enhance customer experience, and optimize restaurant management workflows.

✨ Features
🍔 Menu Management
📋 Menu Creation - Easily create and organize food categories

📊 Item Management - Add, edit, or remove menu items with prices

📸 Photo Upload - Add appealing images for each menu item

🏷️ Special Offers - Create and manage daily specials and promotions

🛒 Order Processing
📝 Order Taking - Efficient order placement system

⏱️ Real-time Tracking - Monitor order status from kitchen to table

🔄 Table Management - Manage table assignments and rotations

💳 Payment Processing - Secure payment handling system

📦 Inventory Control
📊 Stock Monitoring - Real-time inventory tracking

📈 Low Stock Alerts - Automatic notifications for restocking

📋 Supplier Management - Vendor and supplier information

📊 Consumption Analytics - Track ingredient usage patterns

👥 Customer Experience
📱 Customer Profiles - Store customer preferences and history

⭐ Loyalty Program - Rewards and points system

📅 Reservations - Online table booking system

💬 Feedback Collection - Customer reviews and ratings

📊 Reporting & Analytics
💰 Sales Reports - Daily, weekly, monthly revenue tracking

📈 Performance Metrics - Staff efficiency and sales trends

📋 Inventory Reports - Stock usage and wastage analysis

📊 Customer Insights - Popular items and peak hours data

🚀 Tech Stack
🔧 Backend
https://img.shields.io/badge/Python-3.8+-blue?logo=python
https://img.shields.io/badge/Django-4.0+-green?logo=django
https://img.shields.io/badge/Django_REST_Framework-red?logo=django

🎨 Frontend
https://img.shields.io/badge/React-18.0+-61DAFB?logo=react
https://img.shields.io/badge/TypeScript-5.0+-3178C6?logo=typescript
https://img.shields.io/badge/Tailwind_CSS-3.0+-06B6D4?logo=tailwindcss

💾 Database
https://img.shields.io/badge/PostgreSQL-15.0+-4169E1?logo=postgresql
https://img.shields.io/badge/Redis-Cache-DC382D?logo=redis

🛠️ DevOps
https://img.shields.io/badge/Docker-Container-2496ED?logo=docker
https://img.shields.io/badge/GitHub_Actions-CI/CD-2088FF?logo=githubactions
https://img.shields.io/badge/AWS-Deployment-FF9900?logo=amazonaws

📦 Installation
Prerequisites 📋
Python 3.8+ 🐍

Node.js 18+ ⚡

PostgreSQL 15+ 🐘

Redis 7+ 🔴

Git 📌

Step-by-Step Setup 🛠️
Clone the Repository

bash
git clone https://github.com/Lwazi02Hadebe/MaziyaInnPubVleis.git
cd MaziyaInnPubVleis
Set Up Backend

bash
# Create virtual environment
python -m venv venv

# Activate virtual environment
# Windows:
venv\Scripts\activate
# Linux/Mac:
source venv/bin/activate

# Install dependencies
pip install -r requirements.txt

# Set up environment variables
cp .env.example .env
# Edit .env with your configuration

# Run migrations
python manage.py migrate

# Create superuser
python manage.py createsuperuser

# Start development server
python manage.py runserver
Set Up Frontend

bash
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev
Run with Docker (Alternative) 🐳

bash
# Build and run all services
docker-compose up --build

# Run in background
docker-compose up -d

# View logs
docker-compose logs -f
📁 Project Structure
text
MaziyaInnPubVleis/
│
├── 📁 backend/                   # Django backend
│   ├── 📁 api/                   # API endpoints
│   ├── 📁 core/                  # Core functionality
│   ├── 📁 menu/                  # Menu management app
│   ├── 📁 orders/                # Order processing app
│   ├── 📁 inventory/             # Inventory management app
│   ├── 📁 customers/             # Customer management app
│   └── 📁 reports/               # Reporting app
│
├── 📁 frontend/                  # React frontend
│   ├── 📁 src/
│   │   ├── 📁 components/        # Reusable components
│   │   ├── 📁 pages/            # Page components
│   │   ├── 📁 hooks/            # Custom React hooks
│   │   ├── 📁 utils/            # Utility functions
│   │   └── 📁 types/            # TypeScript types
│   │
├── 📁 docker/                    # Docker configuration
├── 📁 docs/                      # Documentation
├── 📁 tests/                     # Test files
├── 📁 scripts/                   # Utility scripts
│
├── 📄 docker-compose.yml         # Docker Compose config
├── 📄 .env.example              # Environment template
├── 📄 requirements.txt          # Python dependencies
├── 📄 package.json              # Node.js dependencies
└── 📄 README.md                 # This file
🔗 API Documentation
Core Endpoints 🌐
Method	Endpoint	Description	Icon
GET	/api/menu/	Get all menu items	📋
POST	/api/menu/	Create new menu item	➕
GET	/api/orders/	List all orders	📝
POST	/api/orders/	Create new order	🛒
GET	/api/inventory/	View inventory	📊
PUT	/api/inventory/{id}/	Update stock	🔄
GET	/api/customers/	Get customer list	👥
POST	/api/reservations/	Make reservation	📅
Sample API Request 💡
bash
# Get all menu items
curl -X GET http://localhost:8000/api/menu/ \
  -H "Authorization: Token YOUR_TOKEN_HERE"

# Create new order
curl -X POST http://localhost:8000/api/orders/ \
  -H "Content-Type: application/json" \
  -H "Authorization: Token YOUR_TOKEN_HERE" \
  -d '{
    "table_number": 5,
    "items": [
      {"menu_item": 1, "quantity": 2},
      {"menu_item": 3, "quantity": 1}
    ]
  }'
🎯 Usage Examples
📱 Customer Portal
Browse Menu - View categorized food items with images

Place Order - Select items and customize orders

Track Order - Real-time order status updates

Make Payment - Secure payment options

👨‍🍳 Staff Dashboard
View Orders - See pending and completed orders

Manage Tables - Assign and clear tables

Update Inventory - Adjust stock levels

Generate Reports - Sales and performance analytics

👨‍💼 Admin Panel
Manage Staff - Add/remove staff accounts

Configure Settings - System configuration

View Analytics - Business insights

Backup Data - System backup and restore

🧪 Testing
bash
# Run backend tests
python manage.py test

# Run frontend tests
cd frontend
npm test

# Run end-to-end tests
npm run test:e2e

# Generate coverage report
npm run test:coverage
🤝 Contributing
We welcome contributions! Here's how you can help:

🐛 Report Bugs

Use GitHub Issues with the "bug" label

Include steps to reproduce

Add screenshots if applicable

💡 Suggest Features

Create an issue with the "enhancement" label

Explain the use case and benefits

🔧 Submit Pull Requests

bash
# Fork the repository
# Create a feature branch
git checkout -b feature/your-feature

# Make your changes
# Add tests for new functionality

# Commit changes
git commit -m "Add: your feature description"

# Push to your fork
git push origin feature/your-feature

# Create Pull Request
📋 Contribution Guidelines
✅ Write clear commit messages

✅ Add tests for new features

✅ Update documentation

✅ Follow existing code style

✅ Keep PRs focused and small

📄 License
This project is licensed under the MIT License - see the LICENSE file for details.

text
MIT License

Copyright (c) 2024 Lwazi Hadebe

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
...
👥 Team
👤 Project Lead
Lwazi Hadebe - GitHub | LinkedIn

🎨 Design Team
UI/UX Designers - Interface design

Frontend Developers - User experience

🔧 Development Team
Backend Developers - API and database

DevOps Engineers - Deployment and infrastructure

📊 Business Team
Product Manager - Requirements and planning

Quality Assurance - Testing and validation

📞 Support
🆘 Need Help?
📖 Documentation - Check the docs folder

❓ FAQ - See common questions and answers

🐛 Bug Reports - Open an issue on GitHub

💬 Discussions - Join our community chat

📧 Contact
Email: project@maziyainn.com

Issues: GitHub Issues

Discussions: GitHub Discussions

🌟 Acknowledgments
🙏 Special Thanks
Maziya Inn Pub team for their valuable insights

All contributors and testers

Open source community for amazing tools

📚 Resources
Django Documentation

React Documentation

PostgreSQL Documentation

Docker Documentation

📊 Project Status
Component	Status	Version	Notes
Backend API	✅ Stable	v1.2.0	Core functionality complete
Frontend UI	✅ Stable	v1.1.0	Responsive design
Database	✅ Stable	v1.0.0	PostgreSQL optimized
Mobile App	🔄 In Progress	v0.5.0	Under development
Admin Panel	✅ Complete	v1.3.0	Full feature set
Reporting	✅ Complete	v1.2.0	Advanced analytics
🚀 Quick Start Commands
bash
# Development
make dev              # Start all services
make migrate         # Run migrations
make test           # Run all tests

# Production
make build          # Build containers
make deploy         # Deploy to production
make backup         # Backup database

# Utilities
make logs           # View logs
make shell          # Open Django shell
make clean          # Clean temporary files
<div align="center">
⭐ Star this repo if you find it useful!
https://img.shields.io/github/stars/Lwazi02Hadebe/MaziyaInnPubVleis?style=social
https://img.shields.io/github/forks/Lwazi02Hadebe/MaziyaInnPubVleis?style=social
https://img.shields.io/github/issues/Lwazi02Hadebe/MaziyaInnPubVleis

Built with ❤️ for Maziya Inn Pub
