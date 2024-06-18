using Basket_Caching_Demo.Interface;
using Basket_Caching_Demo.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Basket_Caching_Demo.Repository
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDatabase _redisDb;
        private const string BasketKeySet = "all_basket_keys";
        public BasketRepository(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }
        public async Task<bool> DeleteBasketAsync(string basketId)
        {
            //  return await _redisDb.KeyDeleteAsync(basketId);
            var wasRemoved = await _redisDb.KeyDeleteAsync(basketId);
            if (wasRemoved)
            {
                await _redisDb.SetRemoveAsync(BasketKeySet, basketId);
            }
            return wasRemoved;
        }

        public async Task<IEnumerable<UserBasket>> GetAllBasketAsync()
        {
            var basketIds = await _redisDb.SetMembersAsync(BasketKeySet);
            var baskets = new List<UserBasket>();

            foreach (var basketId in basketIds)
            {
                var basket = await GetBasketAsync(basketId);
                if (basket != null)
                {
                    baskets.Add(basket);
                }
            }

            return baskets;
        }

        //public async Task<UserBasket> GetAllBasketAsync()
        //{
        //    var basket = await _redisDb.StringGetAsync("*");
        //    return basket.IsNull ? null : JsonSerializer.Deserialize<UserBasket>(basket);
        //}
       
        public async Task<UserBasket?> GetBasketAsync(string basketId)
        {
            // var basket = await _redisDb.StringGetAsync(basketId);
            //return basket.IsNull ? null : JsonSerializer.Deserialize<UserBasket>(basket);
            var basket = await _redisDb.StringGetAsync(basketId);
            return basket.IsNullOrEmpty ? null : JsonSerializer.Deserialize<UserBasket>(basket);

        }
        public async Task<bool> UpdateBasketAsync(UserBasket basket)
        {
            var serializedBasket = JsonSerializer.Serialize(basket);
            var wasSet = await _redisDb.StringSetAsync(basket.Id, serializedBasket);
            if (wasSet)
            {
                await _redisDb.SetAddAsync(BasketKeySet, basket.Id);
            }
            return wasSet;
        }


        public async Task<UserBasket?> UpdateOrCreateBasketAsync(UserBasket basket)
        {
            // if the basketId not exists create new basket else update existing basket
            // the last parameter  expiration time for  the basket 
            //var updateOrCreateBasket = await _redisDb.StringSetAsync(basket.Id, JsonSerializer.Serialize(basket), TimeSpan.FromDays(30));
            //if (updateOrCreateBasket)
            //    return await GetBasketAsync(basket.Id);
           



           // ***********
                 var serializedBasket = JsonSerializer.Serialize(basket);
            var wasSet = await _redisDb.StringSetAsync(basket.Id, serializedBasket);
            if (wasSet)
            {
                await _redisDb.SetAddAsync(BasketKeySet, basket.Id);
            }
           
            return null;
        }
    }
}
