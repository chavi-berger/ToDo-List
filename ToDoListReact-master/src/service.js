import axios from 'axios';

// 1. הגדרת כתובת ברירת מחדל
axios.defaults.baseURL = process.env.REACT_APP_API_URL;

axios.defaults.headers.common['Accept'] = 'application/json';

// 2. הוספת interceptor לשגיאות
axios.interceptors.response.use(
  response => response, // אם הכל בסדר, מחזיר את ה-response
  error => {
    console.error('API Error:', error.response ? error.response.data : error.message);
    return Promise.reject(error); // ממשיך לזרוק את השגיאה למי שקורא את הפונקציה
  }
);

export default {
  getTasks: async () => {
    const result = await axios.get('/items');
    return result.data;
  },

  addTask: async (name) => {
    const newItem = { name, isComplete: false };
    const result = await axios.post('/items', newItem);
    return result.data;
  },

  setCompleted: async (id, isComplete) => {
    const updatedItem = { id, name: '', isComplete };
    const result = await axios.put(`/items/${id}`, updatedItem);
    return result.data;
  },

  deleteTask: async (id) => {
    await axios.delete(`/items/${id}`);
    return { success: true };
  }
};
