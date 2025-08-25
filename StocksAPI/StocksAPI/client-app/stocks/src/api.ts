import axios from 'axios'
import { useAuth0 } from '@auth0/auth0-vue'


export const useApi = () => {
    const auth0 = useAuth0()

    const createAuthenticatedApi = async () => {
        const { getAccessTokenSilently, isAuthenticated } = auth0

        const api = axios.create({
            baseURL: import.meta.env.VITE_API_URL,
        })

        if (isAuthenticated.value) {
            try {
                const token = await getAccessTokenSilently({
                    authorizationParams: {
                        audience: import.meta.env.VITE_AUTH0_AUDIENCE,
                    }
                })
                api.defaults.headers.common['Authorization'] = `Bearer ${token}`
            } catch (error) {
                console.error('Error getting access token:', error)
            }
        }

        return api
    }

    const apiGet = async (url: string, config = {}) => {
        const api = await createAuthenticatedApi()
        return api.get(url, config)
    }

    const apiPost = async (url: string, data = {}, config = {}) => {
        const api = await createAuthenticatedApi()
        return api.post(url, data, config)
    }

    const apiPut = async (url: string, data = {}, config = {}) => {
        const api = await createAuthenticatedApi()
        return api.put(url, data, config)
    }

    const apiDelete = async (url: string, config = {}) => {
        const api = await createAuthenticatedApi()
        return api.delete(url, config)
    }

    return {
        apiGet,
        apiPost,
        apiPut,
        apiDelete
    }
}