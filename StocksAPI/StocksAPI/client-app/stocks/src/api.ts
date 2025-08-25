import axios from 'axios'
import { useAuth0 } from '@auth0/auth0-vue'


// Create base axios instance without interceptors
const baseApi = axios.create({
    baseURL: import.meta.env.VITE_API_URL,
})

// Function to create an authenticated API instance
const createAuthenticatedApi = async () => {
    const { getAccessTokenSilently, isAuthenticated } = useAuth0()

    // Clone the base instance
    const api = axios.create({
        baseURL: import.meta.env.VITE_API_URL,
    })

    // Add auth header if authenticated
    if (isAuthenticated.value) {
        try {
            const token = await getAccessTokenSilently()
            api.defaults.headers.common['Authorization'] = `Bearer ${token}`
        } catch (error) {
            console.error('Error getting access token:', error)
        }
    }

    return api
}

// Wrapper functions for API calls
export const apiGet = async (url: string, config = {}) => {
    const api = await createAuthenticatedApi()
    return api.get(url, config)
}


export const apiPost = async (url: string, data = {}, config = {}) => {
    const api = await createAuthenticatedApi()
    return api.post(url, data, config)
}

export const apiPut = async (url: string, data = {}, config = {}) => {
    const api = await createAuthenticatedApi()
    return api.put(url, data, config)
}

export const apiDelete = async (url: string, config = {}) => {
    const api = await createAuthenticatedApi()
    return api.delete(url, config)
}

export default baseApi