-- Atomik eşleştirme script'i
-- ARGV[1]: playerId
-- ARGV[2]: mmr (score)
-- ARGV[3]: delta
-- ARGV[4]: nowTicks
-- ARGV[5]: matchId

local playerId = ARGV[1]
local mmr = tonumber(ARGV[2])
local delta = tonumber(ARGV[3])
local nowTicks = tonumber(ARGV[4])
local matchId = ARGV[5]

local queueKey = "mmrpg:pvp:queue"
local matchKey = "mmrpg:pvp:match:" .. matchId
local acceptKey = "mmrpg:pvp:accept:" .. matchId

-- Adayları bul (MMR aralığında, kendisi hariç)
local candidates = redis.call('ZRANGEBYSCORE', queueKey, mmr - delta, mmr + delta)

for i, candidate in ipairs(candidates) do
    if candidate ~= playerId then
        -- Her iki oyuncuyu da kuyruktan çıkar
        local removed = redis.call('ZREM', queueKey, playerId, candidate)
        
        if removed >= 2 then
            -- Match oluştur
            redis.call('HSET', matchKey, 
                'p1', playerId,
                'p2', candidate,
                'createdAt', nowTicks,
                'state', 'awaiting'
            )
            
            -- TTL ayarla (5 dakika)
            redis.call('EXPIRE', matchKey, 300)
            redis.call('EXPIRE', acceptKey, 120)
            
            -- Oyuncu durumlarını güncelle
            redis.call('HSET', 'mmrpg:pvp:state:' .. playerId, 'state', 'matched', 'matchId', matchId)
            redis.call('HSET', 'mmrpg:pvp:state:' .. candidate, 'state', 'matched', 'matchId', matchId)
            
            -- Bekleme meta verilerini sil
            redis.call('DEL', 'mmrpg:pvp:wait:' .. playerId)
            redis.call('DEL', 'mmrpg:pvp:wait:' .. candidate)
            
            return {matchId, playerId, candidate}
        end
    end
end

return nil
