const API_BASE_URL = window.location.origin.includes('localhost:5151') ? '/api' : 'http://localhost:5151/api';

const api = {
    // ---- ALUNOS ----
    async getStudents() {
        try {
            const res = await fetch(`${API_BASE_URL}/students`);
            if (!res.ok) throw new Error('Falha ao buscar alunos');
            return await res.json();
        } catch (error) {
            console.error(error);
            return [];
        }
    },
    async createStudent(studentData) {
        const res = await fetch(`${API_BASE_URL}/students`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(studentData)
        });
        return await res.json();
    },
    async updateStudent(id, studentData) {
        await fetch(`${API_BASE_URL}/students/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(studentData)
        });
    },
    async deleteStudent(id) {
        await fetch(`${API_BASE_URL}/students/${id}`, { method: 'DELETE' });
    },

    // ---- QUADRAS ----
    async getCourts() {
        try {
            const res = await fetch(`${API_BASE_URL}/courts`);
            if (!res.ok) return [];
            return await res.json();
        } catch (error) { return []; }
    },
    async createCourt(courtData) {
        const res = await fetch(`${API_BASE_URL}/courts`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(courtData)
        });
        return await res.json();
    },

    // ---- AGENDA (RESERVAS) ----
    async getReservations() {
        try {
            const res = await fetch(`${API_BASE_URL}/reservations`);
            if (!res.ok) return [];
            return await res.json();
        } catch (error) { return []; }
    },
    async createReservation(reservationData) {
        const res = await fetch(`${API_BASE_URL}/reservations`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(reservationData)
        });
        if (!res.ok) {
            const err = await res.text();
            throw new Error(err || 'Falha ao criar reserva');
        }
        return await res.json();
    },

    // ---- BAR / PRODUTOS ----
    async getProducts(search = '', categoryId = '') {
        try {
            let url = `${API_BASE_URL}/products?`;
            if (search) url += `search=${encodeURIComponent(search)}&`;
            if (categoryId) url += `categoryId=${categoryId}&`;
            const res = await fetch(url);
            if (!res.ok) return [];
            return await res.json();
        } catch (e) { return []; }
    },
    async updateProduct(id, productData) {
        const res = await fetch(`${API_BASE_URL}/products/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(productData)
        });
        return res.ok;
    },
    async getProductCategories() {
        try {
            const res = await fetch(`${API_BASE_URL}/productcategories`);
            if (!res.ok) return [];
            return await res.json();
        } catch (e) { return []; }
    },
    async createProduct(productData) {
        const res = await fetch(`${API_BASE_URL}/products`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(productData)
        });
        return await res.json();
    },

    // ---- CLIENTES ----
    async getCustomers() {
        try {
            const res = await fetch(`${API_BASE_URL}/customers`);
            if (!res.ok) return [];
            return await res.json();
        } catch (e) { return []; }
    },
    async createCustomer(customerData) {
        const res = await fetch(`${API_BASE_URL}/customers`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(customerData)
        });
        return await res.json();
    },
    async updateCustomer(id, customerData) {
        const res = await fetch(`${API_BASE_URL}/customers/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(customerData)
        });
        return res.ok;
    },
    async deleteCustomer(id) {
        const res = await fetch(`${API_BASE_URL}/customers/${id}`, { method: 'DELETE' });
        return res.ok;
    },

    // ---- VENDAS ----
    async createSale(saleData) {
        const res = await fetch(`${API_BASE_URL}/sales`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(saleData)
        });
        if (!res.ok) {
            const err = await res.text();
            throw new Error(err || 'Falha ao realizar venda');
        }
        return await res.json();
    },
    async getSales() {
        try {
            const res = await fetch(`${API_BASE_URL}/sales`);
            if (!res.ok) return [];
            return await res.json();
        } catch (e) { return []; }
    },

    // ---- ANALYTICS ----
    async getAnalytics() {
        try {
            const res = await fetch(`${API_BASE_URL}/analytics/dashboard`);
            if (!res.ok) throw new Error('Falha ao buscar analytics');
            return await res.json();
        } catch (e) {
            console.error(e);
            return null;
        }
    }
};

window.api = api;
