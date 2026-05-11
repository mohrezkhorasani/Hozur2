import { useState, useEffect } from 'react';

const useFetch = (url, defaultData = null, options = {}) => {
    const [data, setData] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const fetchData = async () => {
        if (!url) return;

        setLoading(true);
        setError(null);

        try {
            const response = await fetch(url, options);
            if (!response.ok) throw new Error('مشکلی در دریافت داده‌ها وجود دارد.');

            const result = await response.json();
            setData(result);
        } catch (err) {
            setError(err.message);
            setData(defaultData)
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        console.log(url);
        fetchData();
    }, [url]);

    return { data, loading, error, refetch: fetchData };
};
export default useFetch