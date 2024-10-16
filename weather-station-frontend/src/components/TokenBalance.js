import React, { useEffect, useState } from 'react';
import { getTokenBalance } from '../services/tokenService';

const TokenBalance = () => {
    const [tokenBalances, setTokenBalances] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            const sensorAddresses = ['0x...', '0x...']; // Add sensor addresses here
            const balances = await Promise.all(sensorAddresses.map(async address => {
                const balance = await getTokenBalance(address);
                return { address, balance };
            }));
            setTokenBalances(balances);
        };

        fetchData();
    }, []);

    return (
        <div>
            <h1>Token Balances</h1>
            <ul>
                {tokenBalances.map(({ address, balance }) => (
                    <li key={address}>
                        Sensor {address}: {balance} tokens
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default TokenBalance;
