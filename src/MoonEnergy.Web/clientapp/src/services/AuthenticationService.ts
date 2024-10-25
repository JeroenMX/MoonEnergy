import axios from "axios";

export async function getUserName(): Promise<string | null>
{
    const getUser = async () => {
        const config = {
            headers: {
                'X-CSRF': '1'
            }
        }

        return await axios.get('/bff/user', {
            ...config,
            validateStatus: function (status) {
                return true;  // Resolve promise for all HTTP status codes
            }
        });
    }

    const user = await getUser();

    if (user.status === 200) {
        return user.data.find(x => x.type === 'name').value;
    }
    
    return null;
}